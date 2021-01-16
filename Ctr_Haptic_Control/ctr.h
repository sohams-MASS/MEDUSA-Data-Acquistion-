#pragma once

#include "A3200.h"
#include "CtrConfig.h"


using namespace std;

constexpr CtrConfig CTR_CONFIG;

class Ctr {

private:
	CtrConfig _CTR_CONFIG;

	void initA3200();
	void dataAcquisition();

public:
	Ctr();
	~Ctr();
	
	
};