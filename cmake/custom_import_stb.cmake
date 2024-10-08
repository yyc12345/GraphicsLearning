# Check stb path variable
if (NOT DEFINED STB_IMAGE_PATH)
    message(FATAL_ERROR "You must set STB_IMAGE_PATH variable to the root of std-image repository.")
endif()

# Add library
add_library(stb-image INTERFACE IMPORTED)
set_target_properties(stb-image PROPERTIES
INTERFACE_INCLUDE_DIRECTORIES
    "${STB_IMAGE_PATH}"
)
