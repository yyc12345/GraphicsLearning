import bpy,bmesh,bpy_extras,mathutils
import time,os,math
import struct
from bpy_extras import io_utils,node_shader_utils
from bpy_extras.io_utils import unpack_list
from bpy_extras.image_utils import load_image
   
class RADIUM_OT_export_rs(bpy.types.Operator, bpy_extras.io_utils.ExportHelper):
    """Save as Radium scene file (RS file spec 1.3)"""
    bl_idname = "radium.export_rs"
    bl_label = 'Export to Radium'
    bl_options = {'PRESET'}
    filename_ext = ".rs"

    @classmethod
    def poll(self, context):
        return bpy.context.scene.camera is not None

    def execute(self, context):
        export_rs(context, self.filepath)
        return {'FINISHED'}

# ========================================== method

rs_current_version = 0
rs_magic_words = 61

def export_rs(context, filepath):

    # collect data
    objectList = [] # the mesh object list
    lightList = []
    cameraList = []

    for obj in bpy.context.scene.objects:
        if obj.type == 'MESH':
            if obj.data is None:
                continue # clean no mesh object
            objectList.append(obj)
        elif obj.type == 'LIGHT':
            if obj.data.type not in ('SUN', 'POINT'):
                continue # only support parallel and point light
            lightList.append(obj)
        elif obj.type == 'CAMERA':
            if obj.data.type != 'PERSP':
                continue # only support PERSP camera
            cameraList.append(obj)

    with open(filepath, 'wb') as fs:
        write_uint32(fs, rs_magic_words)
        write_uint32(fs, rs_current_version)

        # ====================== write camera
        write_uint32(fs, len(cameraList))
        for oCamera in cameraList:
            # write position
            write_3dvector(fs, oCamera.location)
            # calculate direction and write it
            direction = mathutils.Vector((0, 0, -1))
            direction.rotate(oCamera.rotation_euler)
            write_3dvector(fs, direction)
            # calculate up axis and write it
            up_axis = mathutils.Vector((0, 1, 0))
            up_axis.rotate(oCamera.rotation_euler)
            write_3dvector(fs, up_axis)

            # write clip start and end
            write_float(fs, oCamera.data.clip_start)
            write_float(fs, oCamera.data.clip_end)

            # write angle
            write_float(fs, oCamera.data.angle_x)
            write_float(fs, oCamera.data.angle_y)

        # ====================== write light
        write_uint32(fs, len(lightList))
        for oLight in lightList:
            # write type
            if oLight.data.type == 'POINT':
                # point light
                write_uint8(fs, 0)

                # write position
                write_3dvector(fs, oLight.location)
                # point light don't need direction
                # write color
                write_color(fs, oLight.data.color)
            elif oLight.data.type == 'SUN':
                # sunlight
                write_uint8(fs, 1)

                # sun light don't need location
                # write direction
                direction = mathutils.Vector((0, 0, -1))
                #direction.rotate(oCamera.rotation_euler)
                direction = direction @ oLight.matrix_world
                position = mathutils.Vector(oLight.location)
                direction = direction - position
                direction.normalize()
                write_3dvector(fs, direction)
                # write color
                write_color(fs, oLight.data.color)
            else:
                raise Exception('Unexpected light')

        # ====================== write mesh
        materialSet = set()
        materialList = []   
        write_uint32(fs, len(objectList))
        for oObject in objectList:
            world_matrix = oObject.matrix_world # the matrix used for vectices
            normal_matrix = world_matrix.inverted_safe().transposed().to_3x3()   # the matrix used for normal
            mesh = oObject.data
            mesh_triangulate(mesh)
            mesh.calc_normals_split()

            # write mesh
            # vertices
            vecList = mesh.vertices[:]
            write_uint32(fs, len(vecList))
            for vec in vecList:
                # convert local coordinate -> world coordinate
                write_3dvector(fs,world_matrix @ vec.co)

            # uv
            face_index_pairs = [(face, index) for index, face in enumerate(mesh.polygons)]
            write_uint32(fs, len(face_index_pairs) * 3)
            if mesh.uv_layers.active is not None:
                uv_layer = mesh.uv_layers.active.data[:]
                for f, f_index in face_index_pairs:
                    # it should be triangle face, otherwise throw a error
                    if (f.loop_total != 3):
                        raise Exception("Not a triangle", f.poly.loop_total)

                    for loop_index in range(f.loop_start, f.loop_start + f.loop_total):
                        write_2dvector(fs, uv_layer[loop_index].uv)
            else:
                # no uv data. write garbage
                for i in range(len(face_index_pairs) * 3):
                    write_2dvector(fs, 0.0, 0.0)

            # normals
            write_uint32(fs, len(face_index_pairs) * 3)
            for f, f_index in face_index_pairs:
                # no need to check triangle again
                for loop_index in range(f.loop_start, f.loop_start + f.loop_total):
                    nml = mesh.loops[loop_index].normal
                    write_3dvector(fs, normal_matrix @ nml)

            # face
            # get material first
            currentMat = mesh.materials[:]
            noMaterial = len(currentMat) == 0
            for mat in currentMat:
                if mat not in materialSet:
                    materialSet.add(mat)
                    materialList.append(mat)

            write_uint32(fs, len(face_index_pairs))
            for f, f_index in face_index_pairs:
                # confirm material use
                if noMaterial:
                    usedMat = 0
                else:
                    usedMat = materialList.index(currentMat[f.material_index])

                counter = 0
                for loop_index in range(f.loop_start, f.loop_start + f.loop_total):
                    write_uint32(fs, mesh.loops[loop_index].vertex_index)
                    write_uint32(fs, f_index * 3 + counter)
                    write_uint32(fs, f_index * 3 + counter)
                    counter += 1

                # set used material
                write_bool(fs, not noMaterial)
                write_uint32(fs, usedMat)

            # free splitted normal
            mesh.free_normals_split()

        # write material
        write_uint32(fs, len(materialList))
        textureSet = set()
        textureList = []
        textureCount = 0
        for oMaterial in materialList:
            mat_wrap = node_shader_utils.PrincipledBSDFWrapper(oMaterial)
            if not mat_wrap:
                # no Principled BSDF. write garbage
                material_colAmbient = (1.0, 1.0, 1.0)
                material_colDiffuse = (1.0, 1.0, 1.0)
                material_colSpecular = (1.0, 1.0, 1.0)
                material_specularN = 64.0
                material_kr = 0.0
                material_kt = 0.0

                material_useBaseColorTexture = False
                material_baseColorTexture = 0
                material_useNormalmapTexture = False
                material_normalmapTexture = 0

            else:
                # get basic color
                material_colAmbient = (1.0, 1.0, 1.0)
                material_colDiffuse = (mat_wrap.base_color[0], mat_wrap.base_color[1], mat_wrap.base_color[2])
                material_colSpecular = (mat_wrap.specular, mat_wrap.specular, mat_wrap.specular)
                material_specularN = (1 - mat_wrap.roughness) * 128.0
                material_kr = mat_wrap.metallic
                material_kt = mat_wrap.transmission

                # get base color texture
                base_color_texture = get_material_texture(mat_wrap, "base_color_texture")
                if base_color_texture:
                    if base_color_texture not in textureSet:
                        textureSet.add(base_color_texture)
                        textureList.append(base_color_texture)
                        currentTexture = textureCount
                    else:
                        currentTexture = textureList.index(base_color_texture)

                    material_useBaseColorTexture = True
                    material_baseColorTexture = currentTexture
                else:
                    material_useBaseColorTexture = False
                    material_baseColorTexture = 0


                # get normalmap texture
                normalmap_texture = get_material_texture(mat_wrap, "normalmap_texture")
                if normalmap_texture:
                    if normalmap_texture not in textureSet:
                        textureSet.add(normalmap_texture)
                        textureList.append(normalmap_texture)
                        currentTexture = textureCount
                    else:
                        currentTexture = textureList.index(normalmap_texture)

                    material_useNormalmapTexture = True
                    material_normalmapTexture = currentTexture
                else:
                    material_useNormalmapTexture = False
                    material_normalmapTexture = 0

            write_color(fs, material_colAmbient)
            write_color(fs, material_colDiffuse)
            write_color(fs, material_colSpecular)
            write_float(fs, material_specularN)
            write_float(fs, material_kr)
            write_float(fs, material_kt)
            write_bool(fs, material_useBaseColorTexture)
            write_uint32(fs, material_baseColorTexture)
            write_bool(fs, material_useNormalmapTexture)
            write_uint32(fs, material_normalmapTexture)
        
        # write texture
        write_uint32(fs, len(textureList))
        for oTexture in textureList:
            # write size first
            width = oTexture.size[0]
            height = oTexture.size[1]
            write_uint32(fs, width)
            write_uint32(fs, height)

            counter = 0
            pixels = oTexture.pixels[:]
            for p in pixels:
                # drop alpha channel
                if counter == 3:
                    counter = 0
                else:
                    write_float(fs, p)
                    counter += 1


    '''
    # ============================================ alloc a temp folder
    tempFolderObj = tempfile.TemporaryDirectory()
    tempFolder = tempFolderObj.name
    # debug
    # tempFolder = "G:\\ziptest"
    tempTextureFolder = os.path.join(tempFolder, "Texture")
    os.makedirs(tempTextureFolder)
    prefs = bpy.context.preferences.addons[__package__].preferences
    
    # ============================================ find export target. don't need judge them in there. just collect them
    if export_mode== "COLLECTION":
        objectList = export_target.objects
    else:
        objectList = [export_target]

    # try get forcedCollection
    try:
        forcedCollection = bpy.data.collections[prefs.no_component_collection]
    except:
        forcedCollection = None
   
    # ============================================ export
    with open(os.path.join(tempFolder, "index.bm"), "wb") as finfo:
        write_uint32(finfo, bm_current_version)
        
        # ====================== export object
        meshSet = set()
        meshList = []
        meshCount = 0        
        with open(os.path.join(tempFolder, "object.bm"), "wb") as fobject:
            for obj in objectList:
                # only export mesh object
                if obj.type != 'MESH':
                    continue

                # clean no mesh object
                currentMesh = obj.data
                if currentMesh is None:
                    continue

                # judge component
                object_isComponent = is_component(obj.name)
                object_isForcedNoComponent = False
                if (forcedCollection is not None) and (obj.name in forcedCollection.objects):
                    # change it to forced no component
                    object_isComponent = False
                    object_isForcedNoComponent = True

                # triangle first and then group
                if not object_isComponent:
                    if currentMesh not in meshSet:
                        mesh_triangulate(currentMesh)
                        meshSet.add(currentMesh)
                        meshList.append(currentMesh)
                        meshId = meshCount
                        meshCount += 1
                    else:
                        meshId = meshList.index(currentMesh)
                else:
                    meshId = get_component_id(obj.name)

                # get visibility
                object_isHidden = not obj.visible_get()

                # try get grouping data
                object_groupList = try_get_custom_property(obj, 'virtools-group')
                object_groupList = set_value_when_none(object_groupList, [])

                # write finfo first
                write_string(finfo, obj.name)
                write_uint8(finfo, info_bm_type.OBJECT)
                write_uint64(finfo, fobject.tell())

                # write fobject
                write_bool(fobject, object_isComponent)
                write_bool(fobject, object_isForcedNoComponent)
                write_bool(fobject, object_isHidden)
                write_worldMatrix(fobject, obj.matrix_world)
                write_uint32(fobject, len(object_groupList))
                for item in object_groupList:
                    write_string(fobject, item)
                write_uint32(fobject, meshId)

        # ====================== export mesh
        materialSet = set()
        materialList = []        
        with open(os.path.join(tempFolder, "mesh.bm"), "wb") as fmesh:
            for mesh in meshList:
                mesh.calc_normals_split()

                # write finfo first
                write_string(finfo, mesh.name)
                write_uint8(finfo, info_bm_type.MESH)
                write_uint64(finfo, fmesh.tell())

                # write fmesh
                # vertices
                vecList = mesh.vertices[:]
                write_uint32(fmesh, len(vecList))
                for vec in vecList:
                    #swap yz
                    write_3vector(fmesh,vec.co[0],vec.co[2],vec.co[1])

                # uv
                face_index_pairs = [(face, index) for index, face in enumerate(mesh.polygons)]
                write_uint32(fmesh, len(face_index_pairs) * 3)
                if mesh.uv_layers.active is not None:
                    uv_layer = mesh.uv_layers.active.data[:]
                    for f, f_index in face_index_pairs:
                        # it should be triangle face, otherwise throw a error
                        if (f.loop_total != 3):
                            raise Exception("Not a triangle", f.poly.loop_total)

                        for loop_index in range(f.loop_start, f.loop_start + f.loop_total):
                            uv = uv_layer[loop_index].uv
                            # reverse v
                            write_2vector(fmesh, uv[0], -uv[1])
                else:
                    # no uv data. write garbage
                    for i in range(len(face_index_pairs) * 3):
                        write_2vector(fmesh, 0.0, 0.0)

                # normals
                write_uint32(fmesh, len(face_index_pairs) * 3)
                for f, f_index in face_index_pairs:
                    # no need to check triangle again
                    for loop_index in range(f.loop_start, f.loop_start + f.loop_total):
                        nml = mesh.loops[loop_index].normal
                        # swap yz
                        write_3vector(fmesh, nml[0], nml[2], nml[1])

                # face
                # get material first
                currentMat = mesh.materials[:]
                noMaterial = len(currentMat) == 0
                for mat in currentMat:
                    if mat not in materialSet:
                        materialSet.add(mat)
                        materialList.append(mat)

                write_uint32(fmesh, len(face_index_pairs))
                vtIndex = []
                vnIndex = []
                vIndex = []
                for f, f_index in face_index_pairs:
                    # confirm material use
                    if noMaterial:
                        usedMat = 0
                    else:
                        usedMat = materialList.index(currentMat[f.material_index])

                    # export face
                    vtIndex.clear()
                    vnIndex.clear()
                    vIndex.clear()

                    counter = 0
                    for loop_index in range(f.loop_start, f.loop_start + f.loop_total):
                        vIndex.append(mesh.loops[loop_index].vertex_index)
                        vnIndex.append(f_index * 3 + counter)
                        vtIndex.append(f_index * 3 + counter)
                        counter += 1
                    # reverse vertices sort
                    write_face(fmesh,
                    vIndex[2], vtIndex[2], vnIndex[2],
                    vIndex[1], vtIndex[1], vnIndex[1],
                    vIndex[0], vtIndex[0], vnIndex[0])

                    # set used material
                    write_bool(fmesh, not noMaterial)
                    write_uint32(fmesh, usedMat)

                mesh.free_normals_split()

        # ====================== export material
        textureSet = set()
        textureList = []
        textureCount = 0        
        with open(os.path.join(tempFolder, "material.bm"), "wb") as fmaterial:
            for material in materialList:
                # write finfo first
                write_string(finfo, material.name)
                write_uint8(finfo, info_bm_type.MATERIAL)
                write_uint64(finfo, fmaterial.tell())

                # try get original written data
                material_colAmbient = try_get_custom_property(material, 'virtools-ambient')
                material_colDiffuse = try_get_custom_property(material, 'virtools-diffuse')
                material_colSpecular = try_get_custom_property(material, 'virtools-specular')
                material_colEmissive = try_get_custom_property(material, 'virtools-emissive')
                material_specularPower = try_get_custom_property(material, 'virtools-power')

                # get basic color
                mat_wrap = node_shader_utils.PrincipledBSDFWrapper(material)
                if mat_wrap:
                    use_mirror = mat_wrap.metallic != 0.0
                    if use_mirror:
                        material_colAmbient = set_value_when_none(material_colAmbient, (mat_wrap.metallic, mat_wrap.metallic, mat_wrap.metallic))
                    else:
                        material_colAmbient = set_value_when_none(material_colAmbient, (1.0, 1.0, 1.0))
                    material_colDiffuse = set_value_when_none(material_colDiffuse, (mat_wrap.base_color[0], mat_wrap.base_color[1], mat_wrap.base_color[2]))
                    material_colSpecular = set_value_when_none(material_colSpecular, (mat_wrap.specular, mat_wrap.specular, mat_wrap.specular))
                    material_colEmissive = set_value_when_none(material_colEmissive, mat_wrap.emission_color[:3])
                    material_specularPower = set_value_when_none(material_specularPower, 0.0)

                    # confirm texture
                    tex_wrap = getattr(mat_wrap, "base_color_texture", None)
                    if tex_wrap:
                        image = tex_wrap.image
                        if image:
                            # add into texture list
                            if image not in textureSet:
                                textureSet.add(image)
                                textureList.append(image)
                                currentTexture = textureCount
                                textureCount += 1
                            else:
                                currentTexture = textureList.index(image)

                            material_useTexture = True
                            material_texture = currentTexture
                        else:
                            # no texture
                            material_useTexture = False
                            material_texture = 0
                    else:
                        # no texture
                        material_useTexture = False
                        material_texture = 0

                else:
                    # no Principled BSDF. write garbage
                    material_colAmbient = set_value_when_none(material_colAmbient, (0.8, 0.8, 0.8))
                    material_colDiffuse = set_value_when_none(material_colDiffuse, (0.8, 0.8, 0.8))
                    material_colSpecular = set_value_when_none(material_colSpecular, (0.8, 0.8, 0.8))
                    material_colEmissive = set_value_when_none(material_colEmissive, (0.8, 0.8, 0.8))
                    material_specularPower = set_value_when_none(material_specularPower, 0.0)

                    material_useTexture = False
                    material_texture = 0

                write_color(fmaterial, material_colAmbient)
                write_color(fmaterial, material_colDiffuse)
                write_color(fmaterial, material_colSpecular)
                write_color(fmaterial, material_colEmissive)
                write_float(fmaterial, material_specularPower)
                write_bool(fmaterial, material_useTexture)
                write_uint32(fmaterial, material_texture)
            

        # ====================== export texture
        source_dir = os.path.dirname(bpy.data.filepath)
        existed_texture = set()        
        with open(os.path.join(tempFolder, "texture.bm"), "wb") as ftexture:
            for texture in textureList:
                # write finfo first
                write_string(finfo, texture.name)
                write_uint8(finfo, info_bm_type.TEXTURE)
                write_uint64(finfo, ftexture.tell())

                # confirm internal
                texture_filepath = io_utils.path_reference(texture.filepath, source_dir, tempTextureFolder,
                                                            'ABSOLUTE', "", None, texture.library)
                filename = os.path.basename(texture_filepath)
                write_string(ftexture, filename)
                if (is_external_texture(filename)):
                    write_bool(ftexture, True)
                else:
                    # copy internal texture, if this file is copied, do not copy it again
                    write_bool(ftexture, False)
                    if filename not in existed_texture:
                        shutil.copy(texture_filepath, os.path.join(tempTextureFolder, filename))
                        existed_texture.add(filename)


    # ============================================ save zip and clean up folder
    if os.path.isfile(filepath):
        os.remove(filepath)
    with zipfile.ZipFile(filepath, 'w', zipfile.ZIP_DEFLATED, 9) as zipObj:
       for folderName, subfolders, filenames in os.walk(tempFolder):
           for filename in filenames:
               filePath = os.path.join(folderName, filename)
               arcname=os.path.relpath(filePath, tempFolder)
               zipObj.write(filePath, arcname)
    tempFolderObj.cleanup()
    '''

# ======================================================================================= export assistant

def mesh_triangulate(me):
    bm = bmesh.new()
    bm.from_mesh(me)
    bmesh.ops.triangulate(bm, faces=bm.faces)
    bm.to_mesh(me)
    bm.free()

def get_material_texture(mat_wrap, texture_name):
    tex_wrap = getattr(mat_wrap, texture_name, None)
    if not tex_wrap:
        return None
    image = tex_wrap.image
    if not image:
        return None
    return image

# ======================================================================================= file io assistant

def write_uint8(fs,num):
    fs.write(struct.pack("B", num))

def write_uint32(fs,num):
    fs.write(struct.pack("I", num))

def write_uint64(fs,num):
    fs.write(struct.pack("Q", num))

def write_bool(fs,boolean):
    if boolean:
        write_uint8(fs, 1)
    else:
        write_uint8(fs, 0)

def write_float(fs,fl):
    fs.write(struct.pack("d", fl))

def write_3dvector(fs, vec):
    fs.write(struct.pack("ddd", vec[0], vec[1], vec[2]))

def write_color(fs, colors):
    fs.write(struct.pack("ddd", colors[0], colors[1], colors[2]))

def write_2dvector(fs, uv):
    fs.write(struct.pack("dd", uv[0], uv[1]))

