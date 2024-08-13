# Create static library
add_library(YYCGShared STATIC "")
# Setup source files
target_sources(YYCGShared
PRIVATE
    # Sources
    stbImpls.cpp
    YYCGImage.cpp
)
# Setup header files
target_sources(YYCGShared
PUBLIC
FILE_SET HEADERS
FILES
    # Headers
    YYCGImage.hpp
)
# Setup include directory
target_include_directories(YYCGShared
PUBLIC
    ${CMAKE_CURRENT_LIST_DIR}
    YYCC::YYCCommonplace
PRIVATE
    stb-image
)
# Setup linked library
target_link_libraries(YYCGShared
PUBLIC
    YYCC::YYCCommonplace
PRIVATE
    stb-image
)
# Setup C++ standard
set_target_properties(YYCGShared
PROPERTIES
    CXX_STANDARD 20
    CXX_STANDARD_REQUIRED 20
    CXX_EXTENSION OFF
)
# MSVC specific correction
target_compile_definitions(YYCGShared
PRIVATE
    $<$<CXX_COMPILER_ID:MSVC>:UNICODE>
    $<$<CXX_COMPILER_ID:MSVC>:_UNICODE>
)
target_compile_options(YYCGShared
PRIVATE
    $<$<CXX_COMPILER_ID:MSVC>:/utf-8>
)
