
#define HSE_VALUE ((uint32_t)8000000) /* STM32 discovery uses a 8Mhz external crystal */

#include "stm32f4xx_conf.h"
#include "stm32f4xx.h"
#include "stm32f4xx_gpio.h"
#include "stm32f4xx_rcc.h"
#include "stm32f4xx_exti.h"
#include "usbd_cdc_core.h"
#include "usbd_usr.h"
#include "usbd_desc.h"
#include "usbd_cdc_vcp.h"
#include "usb_dcd_int.h"
#include "main.h"
#include "lcd_5110.h"
#include <stdlib.h>

volatile uint32_t ticker, downTicker;

/*
 * The USB data must be 4 byte aligned if DMA is enabled. This macro handles
 * the alignment, if necessary (it's actually magic, but don't tell anyone).
 */
__ALIGN_BEGIN USB_OTG_CORE_HANDLE  USB_OTG_dev __ALIGN_END;


void init();
void ColorfulRingOfDeath(void);

/*
 * Define prototypes for interrupt handlers here. The conditional "extern"
 * ensures the weak declarations from startup_stm32f4xx.c are overridden.
 */
#ifdef __cplusplus
 extern "C" {
#endif

void SysTick_Handler(void);
void NMI_Handler(void);
void HardFault_Handler(void);
void MemManage_Handler(void);
void BusFault_Handler(void);
void UsageFault_Handler(void);
void SVC_Handler(void);
void DebugMon_Handler(void);
void PendSV_Handler(void);
void OTG_FS_IRQHandler(void);
void OTG_FS_WKUP_IRQHandler(void);

#ifdef __cplusplus
}
#endif

 void pisz(uint8_t theByte, unsigned int ch, int w)
 {
 		LCD5110_set_XY(ch,w);
 		LCD5110_write_char((char)(theByte+0));
 		ch++;
 }

int main(void)
{
	/* Set up the system clocks */
	SystemInit();

	/* Initialize USB, IO, SysTick, and all those other things you do in the morning */
	init();

//LCD
//*************************************************
	  LCD5110_init();

	  LCD5110_set_XY(1, 0);
	  LCD5110_write_string("TEMPERATURY ");
	  LCD5110_set_XY(0, 1);
	  LCD5110_write_string("CPU: ");
	  LCD5110_set_XY(0, 2);
	  LCD5110_write_string("GPU: ");
	  LCD5110_set_XY(0, 3);
	  LCD5110_write_string("HDD: ");
	  unsigned int zn = 0, ch=5;
	  LCD5110_write_string("Zoltem");
	while (1)
	{
		/* Blink the orange LED at 1Hz */
		if (500 == ticker)
		{
			GPIOD->BSRRH = GPIO_Pin_13;
		}
		else if (1000 == ticker)
		{
			ticker = 0;
			GPIOD->BSRRL = GPIO_Pin_13;
		}

		/* If there's data on the virtual serial port:
		 *  - Echo it back
		 *  - Turn the green LED on for 10ms
		 */

		uint8_t theByte;

		if (VCP_get_char(&theByte))
		{
			VCP_put_char(theByte);

//Pisanie przes�anego info
			if(zn>=0 && zn<3)
			{
				pisz(theByte, ch, 1);
				ch++;
				zn++;
			}

			if(zn>=3 && zn<6)
			{
				pisz(theByte, ch, 2);
				ch++;
				zn++;
			}
/*
			if(zn>=9 && zn<11)
			{
				pisz(theByte, ch, 3);
				ch++;
				zn++;
			}
*/
			if(zn==6)
			{
				zn=0;
			}

			GPIOD->BSRRL = GPIO_Pin_12;
			downTicker = 10;
		}
		if (0 == downTicker)
		{
			GPIOD->BSRRH = GPIO_Pin_12;
		}
	}

	return 0;
}

void init()
{
	/* STM32F4 discovery LEDs */
	GPIO_InitTypeDef LED_Config;

	/* Always remember to turn on the peripheral clock...  If not, you may be up till 3am debugging... */
	RCC_AHB1PeriphClockCmd(RCC_AHB1Periph_GPIOD, ENABLE);
	LED_Config.GPIO_Pin = GPIO_Pin_12 | GPIO_Pin_13| GPIO_Pin_14| GPIO_Pin_15;
	LED_Config.GPIO_Mode = GPIO_Mode_OUT;
	LED_Config.GPIO_OType = GPIO_OType_PP;
	LED_Config.GPIO_Speed = GPIO_Speed_25MHz;
	LED_Config.GPIO_PuPd = GPIO_PuPd_NOPULL;
	GPIO_Init(GPIOD, &LED_Config);



	/* Setup SysTick or CROD! */
	if (SysTick_Config(SystemCoreClock / 1000))
	{
		ColorfulRingOfDeath();
	}


	/* Setup USB */
	USBD_Init(&USB_OTG_dev,
	            USB_OTG_FS_CORE_ID,
	            &USR_desc,
	            &USBD_CDC_cb,
	            &USR_cb);

	return;
}

/*
 * Call this to indicate a failure.  Blinks the STM32F4 discovery LEDs
 * in sequence.  At 168Mhz, the blinking will be very fast - about 5 Hz.
 * Keep that in mind when debugging, knowing the clock speed might help
 * with debugging.
 */
void ColorfulRingOfDeath(void)
{
	uint16_t ring = 1;
	while (1)
	{
		uint32_t count = 0;
		while (count++ < 500000);

		GPIOD->BSRRH = (ring << 12);
		ring = ring << 1;
		if (ring >= 1<<4)
		{
			ring = 1;
		}
		GPIOD->BSRRL = (ring << 12);
	}
}

/*
 * Interrupt Handlers
 */

void SysTick_Handler(void)
{
	ticker++;
	if (downTicker > 0)
	{
		downTicker--;
	}
}

void NMI_Handler(void)       {}
void HardFault_Handler(void) { ColorfulRingOfDeath(); }
void MemManage_Handler(void) { ColorfulRingOfDeath(); }
void BusFault_Handler(void)  { ColorfulRingOfDeath(); }
void UsageFault_Handler(void){ ColorfulRingOfDeath(); }
void SVC_Handler(void)       {}
void DebugMon_Handler(void)  {}
void PendSV_Handler(void)    {}

void OTG_FS_IRQHandler(void)
{
  USBD_OTG_ISR_Handler (&USB_OTG_dev);
}

void OTG_FS_WKUP_IRQHandler(void)
{
  if(USB_OTG_dev.cfg.low_power)
  {
    *(uint32_t *)(0xE000ED10) &= 0xFFFFFFF9 ;
    SystemInit();
    USB_OTG_UngateClock(&USB_OTG_dev);
  }
  EXTI_ClearITPendingBit(EXTI_Line18);
}