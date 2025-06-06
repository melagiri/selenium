# Our images must be for Linux x86_64
FROM --platform=linux/amd64 ubuntu:focal@sha256:8e5c4f0285ecbb4ead070431d29b576a530d3166df73ec44affc1cd27555141b

ENV DEBIAN_FRONTEND=noninteractive

# Required for a base build of java targets
RUN apt-get -qqy update && \
    apt-get install -qy --no-install-recommends \
      build-essential \
      libcrypt-dev \
      libnet1 \
      libzip-dev \
      linux-libc-dev \
      python3.10 \
      python-is-python3 \
      zip \
      zlib1g && \
    rm -rf /var/lib/apt/lists/* /var/cache/apt/*

# Make sure we have all the runtime libraries needed by Firefox and Chrome (and so also Edge, one hopes)
RUN apt-get update -qqy && \
    apt-get install -y wget gnupg2 && \
    wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - && echo "deb http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google-chrome.list && \
    install -d -m 0755 /etc/apt/keyrings && \
    wget -q https://packages.mozilla.org/apt/repo-signing-key.gpg -O- | tee /etc/apt/keyrings/packages.mozilla.org.asc > /dev/null && \
    gpg -n -q --import --import-options import-show /etc/apt/keyrings/packages.mozilla.org.asc | awk '/pub/{getline; gsub(/^ +| +$/,""); if($0 == "35BAA0B33E9EB396F59CA838C0BA5CE6DC6315A3") print "\nThe key fingerprint matches ("$0").\n"; else print "\nVerification failed: the fingerprint ("$0") does not match the expected one.\n"}' && \
    echo "deb [signed-by=/etc/apt/keyrings/packages.mozilla.org.asc] https://packages.mozilla.org/apt mozilla main" | tee -a /etc/apt/sources.list.d/mozilla.list > /dev/null && \
    echo '\
    Package: * \
    Pin: origin packages.mozilla.org \
    Pin-Priority: 1000 \
    ' | tee /etc/apt/preferences.d/mozilla > /dev/null && \
    apt-get update -qqy && \
    apt-get -qy install firefox google-chrome-stable && \
    dpkg -r --force-depends firefox google-chrome-stable && \
    apt-get -qy remove wget gnupg2 && \
    rm -rf /var/lib/apt/lists/* /var/cache/apt/*

# Running X Server is a requirement, even if Firefox is running headless
RUN apt-get update -qqy && \
    apt-get install -qy \
      fluxbox \
      libfontconfig \
      libfreetype6 \
      fonts-liberation \
      fonts-ipafont-gothic \
      fonts-wqy-zenhei \
      fonts-tlwg-loma-otf \
      fonts-noto-color-emoji \
      pulseaudio \
      ttf-ubuntu-font-family \
      xfonts-cyrillic \
      xfonts-scalable \
      xvfb && \
    rm -rf /var/lib/apt/lists/* /var/cache/apt/*

COPY start-xvfb.sh /usr/bin/start-xvfb.sh
RUN chmod +x /usr/bin/start-xvfb.sh

RUN useradd -u 108 -ms /bin/bash dev
RUN chmod 777 /
USER dev
WORKDIR /home/dev

ENV LANG C.UTF-8
ENV LC_ALL C.UTF-8
ENV CC /usr/bin/gcc
ENV DISPLAY :99.0

ENTRYPOINT ["/usr/bin/start-xvfb.sh"]
