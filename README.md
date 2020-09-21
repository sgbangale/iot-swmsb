# iot-swmsb - Smart water metering Solution for residents
Dashboard - https://swmsbweb.azurewebsites.net/

# Objective: To create an IoT water consumption measuring system using a simulated device or open source or a public LoRa IoT node/network server and MS Azure IoT Platform for a Smart City project.

# Functional Requirements:

## Register minimum 2 IoT Smart water meters which is connected to single gateway
1. Leverage a simulated node (or) Open source device from your garage (or) a public LoRa device from a live project in India (Candidate to ensure the necessary pre-approvals 
2. permissions are obtained to use their network).
3. In the absence of real IoT node, create MS Azure simulated devices to validate the data flow and real time data processing.
4. Leverage suitable device provisioning methods to register the Smart meter node and gateway
5. Measure the consumption of waters for two homes, ping the data every 5 mins and aggregate data every 30 mins.
		Create below events :
			i. if the consumption is more than 264 gallons  ( 1000 liters)  /per day , then send a push notification to the IOT Platform user “ Your consumption limit has reached the maximum limit for the day” .
			ii. if the consumption difference between two aggregation interval ( i.e within an hour ) is more than 50 gallons, then send a push notification to the IOT Platform user “ Please check the water leak/tap closure in Apt no: XXX”
6. Leverage suitable IoT Protocols for registration, bi-directional communication from device to IoT cloud (and vice versa).
7. Push/Pull various payloads, device telemetry data as applicable for the domestic  Smart water meter.
8. Create a simple IoT dashboard which displays the consumption of the two homes, average consumption by two homes/ per day , etc..
9. Display minimum 2 reports in the dashboard about the water consumptions as applicable for domestic consumer .


## Technical Requirements:

1. Leverage MS Azure IoT Platform for the technical assessment (Trail/ Basic)
2. For LoRa device connectivity Leverage ttnserver (public/free version)
3. Implement MS Azure components appropriately MS IoT hub, Azure functions, suitable DB for hot & cold storage, mobile/email Notification hub, etc.
4  Leverage programming language of your choice, preferably C#, .NET for the Azure functions
5. Leverage Azure DevOps
