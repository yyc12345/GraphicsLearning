#include "YYCGImage.hpp"

#include <stb_image_write.h>
#include <stdexcept>

#if YYCC_OS == YYCC_OS_WINDOWS
#include <WinImportPrefix.hpp>
#include <Windows.h>
#include <WinImportSuffix.hpp>
#endif

namespace YYCG::Shared {

	Image::Image(size_t width, size_t height) :
		m_Image(nullptr), m_Length(0u),
		m_Width(width), m_Height(height) {
		// check argument
		m_Length = m_Width * m_Height;
		if (m_Length == 0u)
			throw std::invalid_argument("image width or height should not be zero.");
		// allocate aligned memory
		m_Image = new img_unit_t[m_Length];
	}

	Image::~Image() {
		if (m_Image != nullptr) {
			delete[] m_Image;
		}
	}

	Image::Image(Image&& rhs) noexcept :
		m_Image(rhs.m_Image), m_Length(rhs.m_Length),
		m_Width(rhs.m_Width), m_Height(rhs.m_Height) {
		rhs.m_Image = nullptr;
		m_Length = m_Width = m_Height = 0u;
	}

	Image& Image::operator=(Image&& rhs) noexcept {
		this->m_Image = rhs.m_Image;
		this->m_Length = rhs.m_Length;
		this->m_Width = rhs.m_Width;
		this->m_Height = rhs.m_Height;

		rhs.m_Image = nullptr;
		m_Length = m_Width = m_Height = 0u;
	}

	struct SaveContext {
		bool* m_IsSuccess;
		YYCC::IOHelper::SmartStdFile* m_Fs;
	};
	static void SaveFunction(void* context, void* data, int size) {
		SaveContext* ctx = static_cast<SaveContext*>(context);
		size_t size_cast = static_cast<size_t>(size);
		*ctx->m_IsSuccess &= (std::fwrite(data, size_cast, 1u, ctx->m_Fs->get()) == size_cast);
	}
	bool Image::Save(const YYCC::yycc_u8string_view& file) {
		// open file
		if (file.empty()) return false;
		YYCC::IOHelper::SmartStdFile fs(YYCC::IOHelper::UTF8FOpen(file.data(), u8"wb"));
		if (fs == nullptr) return false;

		// write into file
		bool is_success = true;
		SaveContext save_ctx { &is_success, &fs };
		stbi_write_png_compression_level = Image::c_PngCompressLevel;
		stbi_write_png_to_func(
			&SaveFunction,
			&save_ctx,
			static_cast<int>(m_Width),
			static_cast<int>(m_Height),
			4, // RGBA8888
			m_Image,
			static_cast<int>(m_Width * 4u) // stride = width * comp
		);

		// free file
		fs.reset();
		// return value
		return is_success;
	}

	void Image::SetColor(size_t x, size_t y, float r, float g, float b, float a) {
#define HELPER(val) static_cast<uint8_t>(std::clamp(val, 0.0f, 1.0f) * std::numeric_limits<uint8_t>::max())
		this->SetColor(x, y, HELPER(r), HELPER(g), HELPER(b), HELPER(a));
#undef HELPER
	}
	void Image::SetColor(size_t x, size_t y, uint8_t r, uint8_t g, uint8_t b, uint8_t a) {
		// check argument
		if (m_Image == nullptr) [[unlikely]]
			throw std::runtime_error("Image is invalid.");
		if (x >= m_Width || y >= m_Height) [[unlikely]]
			throw std::runtime_error("Invalid position.");
		// assign value
		img_unit_t* pixel = m_Image + (x * y);
		pixel->r = r;
		pixel->g = g;
		pixel->b = b;
		pixel->a = a;
	}
}
