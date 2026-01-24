# syntax=docker/dockerfile:1.4
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
ARG CACHEBUST=1
WORKDIR /source
COPY . .


RUN dotnet publish "./vtable.balcony/Vtable.Balcony.csproj" -c Development -r linux-x64 -o /app
RUN ls -R /root/.nuget
RUN dotnet nuget locals all -l


FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy

RUN apt-get update && apt-get install -y \
    libavcodec58 \
    libavformat58 \
    libavutil56 \
    libswscale5 \
    libtiff5 \
    libopenexr25 \
    libtesseract4 \
    libgtk2.0-0 \
    tesseract-ocr \
    git

COPY harden.sh /harden.sh
RUN /bin/bash /harden.sh
WORKDIR /app
COPY --from=build /app .

#RUN cp /app/runtimes/linux-x64/native/libOpenCvSharpExtern.so /app/

ENTRYPOINT ["dotnet", "Vtable.Balcony.dll"]
