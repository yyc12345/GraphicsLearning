add_executable(YYCGPlumbum "")
# Setup source files
target_sources(YYCGPlumbum
PRIVATE
    # Sources
    # Testbench
    Testbench.cpp
    # Algorithm
    LiangBarsky.cpp
)
# Setup header files
target_sources(YYCGPlumbum
PRIVATE
FILE_SET HEADERS
FILES
    # Headers
    YYCGPlumbum.hpp
)
# Setup header infomations
target_include_directories(YYCGPlumbum
PRIVATE
    ${CMAKE_CURRENT_LIST_DIR}
    YYCC::YYCCommonplace
    YYCGShared
)
# Setup linked library
target_link_libraries(YYCGPlumbum
PRIVATE
    YYCC::YYCCommonplace
    YYCGShared
)
# Setup C++ standard
set_target_properties(YYCGPlumbum
PROPERTIES
    CXX_STANDARD 20
    CXX_STANDARD_REQUIRED 20
    CXX_EXTENSION OFF
)
# MSVC specific correction
target_compile_definitions(YYCGPlumbum
PRIVATE
    $<$<CXX_COMPILER_ID:MSVC>:MBCS>
    $<$<CXX_COMPILER_ID:MSVC>:_MBCS>
)
target_compile_options(YYCGPlumbum
PRIVATE
    $<$<CXX_COMPILER_ID:MSVC>:/utf-8>
)

# Install binary and headers
install(TARGETS YYCGPlumbum
    CONFIGURATIONS Release
    RUNTIME DESTINATION ${YYCG_INSTALL_BIN_PATH}
)


