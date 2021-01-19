FROM mono:6

ARG VS_VERSION=1.14.7

RUN mkdir /game
RUN mkdir /code
WORKDIR /game

RUN curl https://cdn.vintagestory.at/gamefiles/stable/vs_server_${VS_VERSION}.tar.gz -o /tmp/server.tar.gz && tar xvf /tmp/server.tar.gz && rm /tmp/server.tar.gz

RUN apt-get update && apt-get install -y git golang zip && apt-get clean
RUN go get -u github.com/tcnksm/ghr

COPY . /code
WORKDIR /code 

RUN mkdir -p /code/lib
RUN cp /game/*.dll /code/lib
RUN cp /game/Mods/* /code/lib
RUN cp /game/Lib/* /code/lib

RUN msbuild resinvessel.csproj -property:Configuration=Release

RUN mkdir /release && cp code/bin/Release/resinvessel/resinvessel.dll /release && cp -r /game/assets /release && cp modinfo.json /release

RUN zip -r /release.zip /release
