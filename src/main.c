#include "../src/xplaneConnect.h"
#include "stdio.h"
#include <stdlib.h>
#include <string.h>
#include <windows.h>

#define MAX_VALUES_SIZE 10

int main(void)
{
	while (1)
	{
		// Connection to Xplane
		XPCSocket client = openUDP("127.0.0.1");

		// Open connection for sending data to Unity (port 49007)
		XPCSocket unityClient = openUDP("127.0.0.1", 49007);

		// Getting Pitch and Roll
		float posi[7];
		int result = getPOSI(client, posi, 0);
		if (result < 0)
		{
			printf("ERROR: getPOSI\n");
			continue;
		}
		printf("Pitch: %f | Roll: %f | Gear: %.1f | ", posi[3], posi[4], posi[6]);

		// Getting other data
		float airSpeed;
		float altitude;
		float heading;
		float qnh;
		float altitudeAGL;
		float engine1, engine2, engine3, engine4;
		const char* drefs[6] = {
			"sim/flightmodel/position/indicated_airspeed",
			"sim/cockpit2/gauges/indicators/altitude_ft_pilot",
			"sim/flightmodel/position/mag_psi",
			"sim/cockpit2/gauges/actuators/barometer_setting_in_hg_pilot",
			"sim/flightmodel/position/y_agl",
			"sim/cockpit2/engine/indicators/prop_speed_rpm"
		};

		float* values[6];

		unsigned char count = 6;
		values[0] = (float*)malloc(1 * sizeof(float));
		values[1] = (float*)malloc(1 * sizeof(float));
		values[2] = (float*)malloc(1 * sizeof(float));
		values[3] = (float*)malloc(1 * sizeof(float));
		values[4] = (float*)malloc(1 * sizeof(float));
		values[5] = (float*)malloc(8 * sizeof(float));

		int sizes[6] = { 1, 1, 1, 1, 1, 8 };

		if (getDREFs(client, drefs, values, count, sizes) < 0)
		{
			printf("An error occured.\n"); //negative return value indicates an error
		}
		else
		{
			printf("Airspeed: %f | ", values[0][0]);
			airSpeed = values[0][0];
			printf("Altitude: %f | ", values[1][0]);
			altitude = values[1][0];
			printf("Heading: %f | ", values[2][0]);
			heading = values[2][0];
			printf("Qnh: %f | ", values[3][0]);
			qnh = values[3][0];
			printf("AltitudeAGL: %f |", values[4][0]);
			altitudeAGL = values[4][0];

			printf("Engine1: %f |", values[5][0]);
			engine1 = values[5][0];
			printf("Engine2: %f |", values[5][1]);
			engine2 = values[5][1];
			printf("Engine3: %f |", values[5][2]);
			engine3 = values[5][2];
			printf("Engine4: %f |\n", values[5][3]);
			engine4 = values[5][3];
		}

		// Creating data string
		char dataToSend[200];
		snprintf(dataToSend, sizeof(dataToSend), "%.6f|%.6f|%.6f|%.6f|%.6f|%.6f|%.6f|%.6f|%.6f|%.6f|%.6f|%.1f|", posi[3], posi[4], airSpeed, altitude, heading, qnh, altitudeAGL, engine1, engine2, engine3, engine4, posi[6]);

		// Setting the address for sending data
		struct sockaddr_in serverAddr;
		serverAddr.sin_family = AF_INET;
		serverAddr.sin_port = htons(49007);

		// Converting an IP address to a format that is usable for the inet_pton function
		if (inet_pton(AF_INET, "127.0.0.1", &serverAddr.sin_addr) <= 0)
		{
			printf("ERROR: Invalid address\n");
			return -1;
		}

		// Sending data
		if (sendto(unityClient.sock, dataToSend, strlen(dataToSend), 0, (struct sockaddr *)&serverAddr, sizeof(serverAddr)) < 0)
		{
			printf("ERROR: Failed to send data\n");
		}

		// Close connection with Xplane
		closeUDP(client);
		// Close connection with Unity
		closeUDP(unityClient);
	}

	return 0;
}
