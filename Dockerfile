FROM mono:6

ARG VS_VERSION=1.14.7

RUN mkdir /game
RUN mkdir /code
WORKDIR /game

RUN curl https://cdn.vintagestory.at/gamefiles/stable/vs_server_${VS_VERSION}.tar.gz -o /tmp/server.tar.gz && tar xvf /tmp/server.tar.gz && rm /tmp/server.tar.gz

RUN apt-get update && apt-get install -y curl git golang zip jq && apt-get clean
RUN go get -u github.com/tcnksm/ghr

COPY . /code
WORKDIR /code 

RUN mkdir -p /code/lib
RUN cp /game/*.dll /code/lib
RUN cp /game/Mods/* /code/lib
RUN cp /game/Lib/* /code/lib

RUN msbuild resinvessel.csproj -property:Configuration=Release

RUN mkdir /release
WORKDIR /release

RUN cp /code/bin/Release/resinvessel/resinvessel.dll . && cp -r /code/assets . && cp /code/modinfo.json .
RUN zip -r /ResinVessel.zip *
RUN cat modinfo.json | jq -r '.["version"]' > /release/version
