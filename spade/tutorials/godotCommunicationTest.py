import spade


class InformBehaviour(spade.behaviour.OneShotBehaviour):

    async def run(self) -> None:
        print("InformBehaviourRunning")
        message = spade.message.Message(to="edblaseTest1@jabbers.one")
        message.set_metadata("performative", "inform")
        message.set_metadata("ontology", "myOntology")
        message.set_metadata("language", "OWL-S")
        message.body = "{\"commandName\":\"TestSendAndReceive\",\"data\":[\"agent1\",\"Tractor\",\"Spawner 1\"]}"

        await self.send(message)
        print("Message sent")

        self.exit_code = 0


class ReceiverBehaviour(spade.behaviour.OneShotBehaviour):

    async def run(self):
        print("Receiver Behaviour Running")

        message = await self.receive(1000)
        if message:
            print("Message received with content: {}".format(message.body))
        else:
            print("No message received")

        await self.agent.stop()


class ReceiverSenderAgent(spade.agent.Agent):

    behaviours = []

    async def setup(self) -> None:
        print("SenderAgent Started")

        receive_behaviour = ReceiverBehaviour()
        self.behaviours.append(receive_behaviour)
        self.add_behaviour(receive_behaviour)

        send_behaviour = InformBehaviour()
        self.behaviours.append(send_behaviour)
        self.add_behaviour(send_behaviour)


async def main():
    sender_agent = ReceiverSenderAgent("edblase@jabbers.one", "6@GBcCqggzER@Gt")
    await sender_agent.start(auto_register=True)
    print("Receiver Started")

    await spade.wait_until_finished(sender_agent)
    print("Agents finished")

if __name__ == "__main__":
    spade.run(main())
