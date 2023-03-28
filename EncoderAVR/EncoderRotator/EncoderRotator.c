/*
 * EncoderRotator.c
 *
 * Created: 09.09.2014 16:50:52
 *  Author: alexander
 */ 

#ifndef F_CPU
#define F_CPU 11059200UL // or whatever may be your frequency
#endif
#define USART_BAUDRATE 9600 
#define BAUD_PRESCALE (((F_CPU / (USART_BAUDRATE * 16UL))) - 1) 

#define DDR(x) (*(&x - 1)) // Address Of Input Register Of Port x
#define PIN(x) (*(&x - 2)) // Address Of Data Direction Register Of Port x

#include <avr/io.h>
#include <avr/interrupt.h>
#include <avr/wdt.h>
#include <avr/eeprom.h>
#include <stdbool.h>
#include <stddef.h>

#define LEDPORT			PORTB
#define LEDMASK			( 1 << 3 )

#define TRPORT			PORTD
#define TRMASK			( 1 << 2 )
#define TR(x) if (x) TRPORT |= TRMASK; else TRPORT &= ~TRMASK

typedef struct  
{
	volatile uint8_t *port;
	unsigned char mask;
} Pin;         			

Pin encPins[] = {
	{ &PORTC, 1 << 1 },
	{ &PORTC, 1 << 2 },
	{ &PORTC, 1 << 3 },
	{ &PORTC, 1 << 4 },
	{ &PORTC, 1 << 5 },
	{ &PORTC, 1 << 6 },
	{ &PORTC, 1 << 7 },
	{ &PORTA, 1 << 7 },
	{ &PORTA, 1 << 6 },
	{ &PORTA, 1 << 5 }
};
const unsigned char encPinCo = 10;

Pin leds[] = {
	{ &PORTB, 1 << 3 },
	{ &PORTB, 1 << 2 }
};
const unsigned char ledCo = 2;


volatile unsigned char loVal = 0;

unsigned int encGrayVal = 0;


void send() {
	unsigned char hiVal = 128 + ( encGrayVal >> 5 );
	loVal = 64 + ( encGrayVal & 31 );
	UDR = hiVal;
}


ISR(TIMER1_COMPA_vect) {
	static unsigned char sendCo = 0;	
	static uint16_t ledBlinkCo = 0;
	wdt_reset();
	if ( sendCo++ > 100 ) {
		send();
		sendCo = 0;		
	}
	if ( ledBlinkCo++ == 500 )
		*leds[0].port |= leds[0].mask;
	else if ( ledBlinkCo >= 1000 ) {
		ledBlinkCo = 0;
		*leds[0].port &= ~leds[0].mask;
		*leds[1].port &= ~leds[1].mask;
	}
}

ISR( USART_TXC_vect )
{
	if ( loVal ) {
		UDR = loVal;
		loVal = 0;
	}		
}



int main(void)
{
unsigned char co;	
// Input/Output Ports initialization
for (co = 0; co < encPinCo; co++ ) {
	DDR( *encPins[co].port ) &= ~encPins[co].mask;
	*encPins[co].port |= encPins[co].mask;
}	

for (co = 0; co < ledCo; co++ ) {
	DDR( *leds[co].port ) |= leds[co].mask;
	*leds[co].port &= ~leds[co].mask;
}	

	
DDR( TRPORT ) |= TRMASK;
TR(1); //TR to T	

// Timer/Counter 1 initialization
// Clock source: System Clock
// Clock value: 43,200 kHz
// Mode: CTC top=OCR1A
// OC1A output: Disconnected
// OC1B output: Disconnected
// Noise Canceler: Off
// Input Capture on Falling Edge
// Timer Period: 1,0185 ms
// Timer1 Overflow Interrupt: Off
// Input Capture Interrupt: Off
// Compare A Match Interrupt: On
// Compare B Match Interrupt: Off
TCCR1A=(0<<COM1A1) | (0<<COM1A0) | (0<<COM1B1) | (0<<COM1B0) | (0<<WGM11) | (0<<WGM10);
TCCR1B=(0<<ICNC1) | (0<<ICES1) | (0<<WGM13) | (1<<WGM12) | (1<<CS12) | (0<<CS11) | (0<<CS10);
TCNT1H=0x00;
TCNT1L=0x00;
ICR1H=0x00;
ICR1L=0x00;
OCR1AH=0x00;
OCR1AL=0x2B;
OCR1BH=0x00;
OCR1BL=0x00;

// Timer(s)/Counter(s) Interrupt(s) initialization
TIMSK=0x10;

//UART initialization
UCSRB = (1 << TXEN);   // Turn on the transmission
UCSRC = (1 << UCSZ0) | (1 << UCSZ1); // Use 8-bit character sizes 

UBRRH = (BAUD_PRESCALE >> 8); // Load upper 8-bits of the baud rate value into the high byte of the UBRR register 
UBRRL = BAUD_PRESCALE; // Load lower 8-bits of the baud rate value into the low byte of the UBRR register 

UCSRB |= ( 1 << TXCIE ); //enable USART RX TX interrupts



wdt_enable( WDTO_2S );


// Global enable interrupts
sei();

for(;;)
{
	unsigned int encGrayValCache = 0;
	unsigned char pinCo = encPinCo - 1;
	do {
		if ( !( PIN( *encPins[pinCo].port ) & encPins[pinCo].mask ) )
			encGrayValCache |= ( 1 << pinCo );
	} while ( pinCo-- > 0 );
	if ( encGrayVal != encGrayValCache )
	{
		encGrayVal = encGrayValCache;
		*leds[1].port |= leds[1].mask;
	}
}
}