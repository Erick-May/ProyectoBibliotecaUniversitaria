import subprocess
import webbrowser
import threading
import time
import sys

def abrir_navegador():
    time.sleep(2)
    webbrowser.open("http://localhost:5087/Index.html")

if __name__ == "__main__":
    print("====================================================")
    print("🏛️  SISTEMA DE BIBLIOTECA UNI - INICIANDO...")
    print("====================================================")
    print("URL: http://localhost:5087/Index.html")
    print("Presiona Ctrl + C en la terminal para detener el servidor.\n")
    
    threading.Thread(target=abrir_navegador, daemon=True).start()
    
    try:
        subprocess.run(["dotnet", "run", "--project", "BibliotecaAPI"])
    except KeyboardInterrupt:
        print("\nServidor detenido correctamente.")
    except Exception as e:
        print(f"\nError al iniciar: {e}")
