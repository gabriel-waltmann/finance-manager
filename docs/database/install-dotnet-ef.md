# Install dotnet ef for arch linux and zsh
```bash
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef

sudo nano ~/.zshrc 
export PATH="$PATH:$HOME/.dotnet/tools"
source ~/.zshrc 
```