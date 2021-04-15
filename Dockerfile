#FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
FROM mcr.microsoft.com/dotnet/sdk:5.0
ARG TZ=Asia/Taipei
RUN     echo "deb http://security.ubuntu.com/ubuntu trusty-security main" >> /etc/apt/sources.list \
    &&  ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
WORKDIR /app
ENTRYPOINT ["./dotnet_run.sh"]
#ENTRYPOINT ["tail", "-f" , "/dev/null"]