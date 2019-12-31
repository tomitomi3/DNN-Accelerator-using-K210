import lcd
import image
import time
import uos
import gc
from Maix import GPIO
from fpioa_manager import *
import sensor
import KPU as kpu
from fpioa_manager import fm
from machine import UART

#------------------------------------------------------------------------------
# refference boot.py
# https://github.com/sboger/m5stickv-firmware-files/blob/master/boot.py
#------------------------------------------------------------------------------
lcd.init()
lcd.rotation(2) #Rotate the lcd 180deg

fm.register(board_info.BUTTON_A, fm.fpioa.GPIO1)
but_a=GPIO(GPIO.GPIO1, GPIO.IN, GPIO.PULL_UP) #PULL_UP is required here!

if but_a.value() == 0: #If dont want to run the demo
    sys.exit()

fm.register(board_info.BUTTON_B, fm.fpioa.GPIO2)
but_b = GPIO(GPIO.GPIO2, GPIO.IN, GPIO.PULL_UP) #PULL_UP is required here!

fm.register(board_info.LED_W, fm.fpioa.GPIO3)
led_w = GPIO(GPIO.GPIO3, GPIO.OUT)
led_w.value(1) #RGBW LEDs are Active Low

fm.register(board_info.LED_R, fm.fpioa.GPIO4)
led_r = GPIO(GPIO.GPIO4, GPIO.OUT)
led_r.value(1) #RGBW LEDs are Active Low

fm.register(board_info.LED_G, fm.fpioa.GPIO5)
led_g = GPIO(GPIO.GPIO5, GPIO.OUT)
led_g.value(1) #RGBW LEDs are Active Low

fm.register(board_info.LED_B, fm.fpioa.GPIO6)
led_b = GPIO(GPIO.GPIO6, GPIO.OUT)
led_b.value(1) #RGBW LEDs are Active Low

# センサーの初期化
err_counter = 0

while 1:
    try:
        sensor.reset() #Reset sensor may failed, let's try sometimes
        break
    except:
        err_counter = err_counter + 1
        if err_counter == 20:
            lcd.draw_string(lcd.width()//2-100,lcd.height()//2-4, "Error: Sensor Init Failed", lcd.WHITE, lcd.RED)
        time.sleep(0.1)
        continue

#https://docs.openmv.io/library/omv.sensor.html
sensor.set_pixformat(sensor.RGB565) #sensor.GRAYSCALE
sensor.set_framesize(sensor.QVGA) #QVGA=320x240
sensor.run(1)

task = kpu.load(0x300000) # Load Model File from Flash
anchor = (1.889, 2.5245, 2.9465, 3.94056, 3.99987, 5.3658, 5.155437, 6.92275, 6.718375, 9.01025)
# Anchor data is for bbox, extracted from the training sets.
kpu.init_yolo2(task, 0.5, 0.3, 5, anchor)

but_stu = 1

# URAT 初期化
fm.register(35, fm.fpioa.UART1_TX, force=True)
fm.register(34, fm.fpioa.UART1_RX, force=True)
#uart1 = UART(UART.UART1, 115200,8,0,0, timeout=1000, read_buf_len=4096)
#uart1 =  UART(UART.UART1, 921600,8,0,0, timeout=1000, read_buf_len=4096)
# 3.6Mbaudrate
uart1 =  UART(UART.UART1, 3686400,8,0,0, timeout=1000, read_buf_len=4096)

#------------------------------------------------------------------------------
# Main loop
#------------------------------------------------------------------------------
try:
    #create blank image object
    # https://docs.openmv.io/library/omv.image.html?highlight=rgb565#class-image-image-object
    img = sensor.snapshot() # Take an image from sensor
    sensorarea = 240*180
    totalssize = sensorarea*3
    lcd.clear((0,0,0))
    sensor.run(0)

    #gc https://maixpy.sipeed.com/en/libs/standard/gc.html
    gc.enable()
    print(gc.mem_free())

    #loop
    while(True):
        # 受信
        read_data = uart1.read()

        # 受信サイズが既定のものかで判定
        if read_data and len(read_data)==totalssize:
            #recv size
            print(len(read_data))

            #KPU利用のためimage objectを書き換え
            for x in range(0, sensorarea):
                i = x * 3
                #RGB565
                r = read_data[i+2];
                g = read_data[i+1];
                b = read_data[i];
                img[x] = [r,g,b] #BGR -> RGB opencvで撮った画像の配列のため入れ替え。PC側で行うべき。

            #推論
            bbox = kpu.run_yolo2(task, img) # Run the detection routine
            count = 0
            if bbox:
                for i in bbox:
                    count = count + 1
                    print(i)
                    img.draw_rectangle(i.rect())

            #推論結果の通知 コンソールで確認
            print(count)

            #display
            lcd.display(img)

        # LED
        if but_a.value() == 0 and but_stu == 1:
            if led_w.value() == 1:
                led_w.value(0)
            else:
                led_w.value(1)
            but_stu = 0
        if but_a.value() == 1 and but_stu == 0:
            but_stu = 1

except KeyboardInterrupt:
    a = kpu.deinit(task)
    sys.exit()
