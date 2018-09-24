using System;

enum Phase {
	calibrating,
	calibrate
}

class FilterV3 {
	Phase mode = Phase.calibrating;

	int countGood = 0;
	int countBad = 0;

	short lastValueX, lastValueY, lastValueZ;
	double lastValueDoubleX, lastValueDoubleY, lastValueDoubleZ;
	double sommeX, sommeY, sommeZ;
	double resultX, resultY, resultZ;

	bool good = false;

	double ecartMax = 0.05;
	int calibrateDuration = 3000;  //30 sec
	short percentGood = 95;
	short vitMin = 16; // x/32768*2000(°/s)

	public void setEcartMax(double ecartMax) {
		this.ecartMax = Math.Abs(ecartMax);
		countGood = 0;
		countBad = 0;
	}

	public void setCalibrateDuration(int duration) {
		this.calibrateDuration = duration;
		countGood = 0;
		countBad = 0;
	}

	public void setPercentGood(short percent) {
		if (percent > 100)
			percent = 100;
		this.percentGood = percent;
		countGood = 0;
		countBad = 0;
	}

	public void addValue(short valX, short valY, short valZ) {
		double vitNorm = Math.Sqrt(valX * valX + valY * valY + valZ * valZ);

		lastValueX = valX;
		lastValueY = valY;
		lastValueZ = valZ;

		if (vitNorm == 0) {
			lastValueDoubleX = 0;
			lastValueDoubleY = 0;
			lastValueDoubleZ = 0;
			good = false;
		}
		else {
			lastValueDoubleX = valX / vitNorm;
			lastValueDoubleY = valY / vitNorm;
			lastValueDoubleZ = valZ / vitNorm;
			if (vitNorm < vitMin)
				good = false;
			else
				good = (Math.Abs(lastValueDoubleX - resultX) <= ecartMax) &&
						(Math.Abs(lastValueDoubleY - resultY) <= ecartMax) &&
						(Math.Abs(lastValueDoubleZ - resultZ) <= ecartMax);
		}

		if (mode == Phase.calibrating) {
			if (good){
				countGood++;
				sommeX += lastValueDoubleX;
				sommeY += lastValueDoubleY;
				sommeZ += lastValueDoubleZ;
				resultX = sommeX / countGood;
				resultY = sommeY / countGood;
				resultZ = sommeZ / countGood;
			}
			else
				countBad++;

			if (getGoodPercent() < percentGood) {
				countGood = 1;
				countBad = 0;
				sommeX = lastValueDoubleX;
				sommeY = lastValueDoubleY;
				sommeZ = lastValueDoubleZ;
				resultX = sommeX;
				resultY = sommeY;
				resultZ = sommeZ;
			}

			if (countGood + countBad > calibrateDuration)
				mode = Phase.calibrate;
		}
	}

	public short getLastValue(int axe) {
		switch (axe) {
			case 0:
				return lastValueX;
			case 1:
				return lastValueY;
			case 2:
				return lastValueZ;
			default:
				return 0;
		}
	}

	public double getLastValueDouble(int axe) {
		switch (axe) {
			case 0:
				return lastValueDoubleX;
			case 1:
				return lastValueDoubleY;
			case 2:
				return lastValueDoubleZ;
			default:
				return 0;
		}
	}
	public double getResult(int axe) {
		switch (axe) {
			case 0:
				return resultX;
			case 1:
				return resultY;
			case 2:
				return resultZ;
			default:
				return 0;
		}
	}
	public int getGood() {
		return good ? 1 : 0;
	}
	public int getCountGood() {
		return countGood;
	}
	public int getCountBad() {
		return countBad;
	}
	public int getGoodPercent() {
		return countGood + countBad == 0 ? 0 : 100 * countGood / (countGood + countBad);
	}
	public int getCalibrationState() {
		return mode == Phase.calibrate ? 1 : 0;
	}
}
