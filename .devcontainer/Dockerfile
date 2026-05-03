FROM mcr.microsoft.com/devcontainers/dotnet:8.0

RUN set -eux; \
		apt-get update; \
		apt-get install -y --no-install-recommends ca-certificates curl; \
		rm -rf /var/lib/apt/lists/*; \
		arch="$(dpkg --print-architecture)"; \
		case "$arch" in \
			amd64) act_arch="x86_64" ;; \
			arm64) act_arch="arm64" ;; \
			*) echo "Unsupported architecture: $arch" >&2; exit 1 ;; \
		esac; \
		curl --fail --silent --show-error --location \
			"https://github.com/nektos/act/releases/download/v0.2.88/act_Linux_${act_arch}.tar.gz" \
			--output /tmp/act.tar.gz; \
		tar -xzf /tmp/act.tar.gz -C /usr/local/bin act; \
		chmod +x /usr/local/bin/act; \
		rm -f /tmp/act.tar.gz

# [Optional] Uncomment this section to install additional OS packages.
# RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
#     && apt-get -y install --no-install-recommends <your-package-list-here>

# [Optional] Uncomment this line to install global node packages.
# RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g <your-package-here>" 2>&1