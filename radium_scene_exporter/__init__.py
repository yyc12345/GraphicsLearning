bl_info={
	"name":"Radium Scene Export Plugin",
	"description":"Export scene for Radium ray tracing render.",
	"author":"yyc12345",
	"version":(1,0),
	"blender":(2,83,0),
	"category":"Import-Export",
	"support":"TESTING",
    "warning": "This is a homemade plugin and only served for Radium render.",
    "wiki_url": "https://code.blumia.cn/yyc12345/Radium",
    "tracker_url": "https://code.blumia.cn/yyc12345/Radium/issues"
}

# ============================================= import system
import bpy,bpy_extras
import bpy.utils.previews
import os
# import my code (with reload)
if "bpy" in locals():
    import importlib
    if "radium_export" in locals():
        importlib.reload(radium_export)
from . import radium_export

# ============================================= blender call system

classes = (
    radium_export.RADIUM_OT_export_rs,
)

def menu_func_bm_export(self, context):
    self.layout.operator(radium_export.RADIUM_OT_export_rs.bl_idname, text="Radium Scene (.rs)")

def register():
    for cls in classes:
        bpy.utils.register_class(cls)
        
    bpy.types.TOPBAR_MT_file_export.append(menu_func_bm_export)
        
def unregister():
    bpy.types.TOPBAR_MT_file_export.remove(menu_func_bm_export)

    for cls in classes:
        bpy.utils.unregister_class(cls)
    
if __name__=="__main__":
	register()