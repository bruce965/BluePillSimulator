public static class Adafruit_SSD1306_h
{
    /// fit into the SSD1306_ naming scheme
    public const uint8_t SSD1306_BLACK = 0;   ///< Draw 'off' pixels
    public const uint8_t SSD1306_WHITE = 1;   ///< Draw 'on' pixels
    public const uint8_t SSD1306_INVERSE = 2; ///< Invert pixels

    public const uint8_t SSD1306_EXTERNALVCC = 0x01;  ///< External display voltage source
    public const uint8_t SSD1306_SWITCHCAPVCC = 0x02; ///< Gen. display voltage from 3.3V
}
