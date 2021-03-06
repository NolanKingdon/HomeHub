[![Actions Status](https://github.com/NolanKingdon/HomeHub/workflows/.NET%20Core/badge.svg)](https://github.com/NolanKingdon/HomeHub/actions)

# HouseHub

Powered by [RaspberryPi](https://www.raspberrypi.org/).

A central hub for several planned functions aimed at improving efficiency and accessibility around my house.

Contains several projects aimed to create a maintainable, testable, and scalable playground for ideas and technologies.

Leverages ASP.NET Core to create an MVC web application to run on my local network that provides several functions designed to improve efficiency and increase connectivity between my devices.

Levereges [Serilog](https://serilog.net/) for local logging and the [Serilog Email Sink](https://github.com/serilog/serilog-sinks-email) for significant errors.

## Projects

### HomeHub.SystemUtils
- Class Library used for interacting with the OS itself
- Allows user to get reads of the system temperature

### HomeHub.SpotifySort
- Runs as a background service.
- Periodically polls Spotify to sort liked songs into predefined playlists by genre.
- [More here](https://github.com/NolanKingdon/HomeHub/tree/master/HomeHub.SpotifySort)

### HomeHub.Tests
- Unit testing using XUnit
- Uses Autofixture for IoC
- Uses AutoMoq/Moq for class Mocking

## Future Projects

### File Services (TODO)
- Will exist as a way to download files via API POST call.

### Api (TODO)
- Future Arduinos
- Download helper (VPN)
- Get logs