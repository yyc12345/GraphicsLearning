cmake_minimum_required(VERSION 3.23)
project(YYCGrahpics
    VERSION 1.0.0
    LANGUAGES CXX
)

# Setup install path
include(GNUInstallDirs)
set(YYCG_INSTALL_BIN_PATH ${CMAKE_INSTALL_BINDIR} CACHE PATH
"Binary install path relative to CMAKE_INSTALL_PREFIX unless set to an absolute path.")

# Import essential library
include(${CMAKE_CURRENT_LIST_DIR}/cmake/custom_import_yycc.cmake)
include(${CMAKE_CURRENT_LIST_DIR}/cmake/custom_import_stb.cmake)

# Import build targets
add_subdirectory(shared)
#add_subdirectory(plumbum)
#add_subdirectory(radium)
