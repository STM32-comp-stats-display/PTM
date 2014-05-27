//--------------------------------------------------------------
// File     : lcd_5110.h
//--------------------------------------------------------------


//--------------------------------------------------------------
#ifndef __STM32F4_LCD_5110_H
#define __STM32F4_LCD_5110_H




//--------------------------------------------------------------
// Includes
//--------------------------------------------------------------
#include "stm32f4xx_gpio.h"
#include "stm32f4xx_rcc.h"



//--------------------------------------------------------------
// Define fuer Hardware oder Software SPI
// (eins von beiden auskommentieren)
//--------------------------------------------------------------
#define USE_Hard_SPI  1    // SPI per SPI-Modul der CPU
//#define USE_Soft_SPI  1  // SPI per GPIO Pins


//--------------------------------------------------------------
#define	LCD_COMMAND	0
#define LCD_DATA        1




//--------------------------------------------------------------
// Globale Funktionen
//--------------------------------------------------------------
void LCD5110_init(void);
void LCD5110_write_char(unsigned char c);
void LCD5110_write_char_inv(unsigned char c);
void LCD5110_write_string(char *s);
void LCD5110_clear(void);
void LCD5110_set_XY(unsigned char X, unsigned char Y);
void LCD5110_write_Dec(unsigned int buffer);
void LCD5110_Led(unsigned char c);



//--------------------------------------------------------------
#endif // __STM32F4_LCD_5110_H
