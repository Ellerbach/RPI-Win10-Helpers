# RPI-Win10-Helpers
Windows 10 IoT Core helpers for Raspberry and other boards.
Those helpers are split over:
- communication
    - HX711, it's a 24 bit analog to digital, 2 analog ways. Using 2 digital GPIO  
    - OneWire (1-wire), using the serial port and 2 wires but making it working :-)
- Movement
    - PWM can use any GPIO. Warning: class not made to use more than 1 or 2 PWM so far
    - Servo Motor to pilot as many servo motors as you want. Warning: precision is not perfect as it's a soft PWM
- Sensors
    - DS18B201 familly. It's using the OneWire library. Make it easy toimplement any other 1 Wire elements
- Web
    - A class to create parameters from URL like ?elam1=abc&elem2=123
- Helplers which does countains precise clocks as well as file system path management

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
