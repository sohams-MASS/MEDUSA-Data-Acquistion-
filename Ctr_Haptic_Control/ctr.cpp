#include <iostream>
#include <sstream>
#include <vector>
#include <thread>
#include <mutex>

#include "ctr.h"

using namespace std;

Ctr::Ctr() :
	_CTR_CONFIG(CTR_CONFIG)
{
	initA3200();

}

Ctr::~Ctr() {
}

void Ctr::initA3200() {}

void Ctr::dataAcquisition() {
	
}