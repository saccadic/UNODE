#include <node.h>
#include "MakePNG.h"

using namespace v8;

Handle<Value> png(const Arguments& args) {
	HandleScope scope;
	png_image png_data;


	return scope.Close(Undefined());
}

Handle<Value> write_png_gray(const Arguments& args) {
	HandleScope scope;
	Png png;
	int w, h, i, j;
	unsigned char   **raw;

	w = args[0]->Int32Value();
	h = args[1]->Int32Value();
	String::Utf8Value file(args[2]->ToString());

	printf("W:%d,H:%d,F:%s\n",w,h,*file);
	
	raw = new ui8*[h];
	for (i = 0; i < h; i++){
		raw[i] = new ui8[w];
	}

	std::cout << "start" << std::endl;

	Local<Array> data = Local<Array>::Cast(args[3]);
	for (i = 0; i < h; i++) {                           // 以下５行は単純なテストパターンを作ります
		for (j = 0; j < w; j++) {
			raw[i][j] = (ui8)(data->Get(j + (w*i))->Int32Value());
		}
	}

	png.init();
	png.write_png_gray(w, h,*file,raw);

	std::cout << "end" << std::endl;

	return scope.Close(Undefined());
}

Handle<Value> write_png_rgb(const Arguments& args) {
	HandleScope scope;
	Png png;
	int w, h, i, j;
	unsigned char   **raw;

	w = args[0]->Int32Value();
	h = args[1]->Int32Value();
	String::Utf8Value file(args[2]->ToString());

	printf("W:%d,H:%d,F:%s\n", w, h, *file);

	raw = new ui8*[h];
	for (i = 0; i < h; i++){
		raw[i] = new ui8[w*3];
	}

	std::cout << "start" << std::endl;

	Local<Array> data = Local<Array>::Cast(args[3]);
	for (i = 0; i < h; i++) {                           // 以下５行は単純なテストパターンを作ります
		for (j = 0;j<w*3;j+=3) {
			raw[i][j  ] = (ui8)(data->Get(j   + (w*3*i))->Int32Value());
			raw[i][j+1] = (ui8)(data->Get(j+1 + (w*3*i))->Int32Value());
			raw[i][j+2] = (ui8)(data->Get(j+2 + (w*3*i))->Int32Value());
		}
	}

	png.init();
	png.write_png_rgba(w, h, *file, raw);

	std::cout << "end" << std::endl;

	return scope.Close(Undefined());
}

void init(Handle<Object> exports) {
	exports->Set(String::NewSymbol("png"),FunctionTemplate::New(png)->GetFunction());
	exports->Set(String::NewSymbol("write_png_gray"), FunctionTemplate::New(write_png_gray)->GetFunction());
	exports->Set(String::NewSymbol("write_png_rgb"), FunctionTemplate::New(write_png_rgb)->GetFunction());
}

NODE_MODULE(png, init)