
# 01/2016 Torsten Brischalle

CC = g++

CFLAGS = -fPIC -O3 -std=c++11

LIBS = -lzim

OBJ =   libzim_csharp.o

all: libzim_csharp.so

libzim_csharp.so: $(OBJ)
	$(CC) -shared -Wl,-soname,libzim_csharp.so.1 -o libzim_csharp.so.1.0 $(OBJ) $(LIBS)

clean:
	rm -f $(OBJ) core

new:
	rm -f libzim_csharp.so libzim_csharp.so.*
	$(MAKE) clean
	$(MAKE) all

%.o:	%.cpp
	$(CC) $(CFLAGS) -c $< -o $@

