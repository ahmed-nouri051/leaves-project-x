 

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src


COPY ["technicalTest.csproj", "."]
RUN dotnet restore "technicalTest.csproj"


COPY . .


RUN dotnet build "technicalTest.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "technicalTest.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .


ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/leavemanagement.db"


RUN mkdir -p /app/data
EXPOSE 80


#VOLUME /app/data


ENTRYPOINT ["dotnet", "technicalTest.dll"]