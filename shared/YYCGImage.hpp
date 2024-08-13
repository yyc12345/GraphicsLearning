#pragma once
#include <YYCCommonplace.hpp>
#include <cinttypes>
#include <numeric>

namespace YYCG::Shared {

	class Image {
	public:
		Image(size_t width, size_t height);
		~Image();
		Image(Image&& rhs) noexcept ;
		Image& operator=(Image&& rhs) noexcept ;
		YYCC_DEL_CLS_COPY(Image);

		bool Save(const YYCC::yycc_u8string_view& file);
		void SetColor(size_t x, size_t y, float r, float g, float b, float a = 0.0f);
		void SetColor(size_t x, size_t y, uint8_t r, uint8_t g, uint8_t b, uint8_t a = std::numeric_limits<uint8_t>::max());

	private:
		static constexpr int c_PngCompressLevel = 3;
#pragma pack(1)
		struct img_unit_t {
			uint8_t r;
			uint8_t g;
			uint8_t b;
			uint8_t a;
		};
#pragma pack()

		img_unit_t* m_Image;
		size_t m_Length;
		size_t m_Width, m_Height;
	};

}
