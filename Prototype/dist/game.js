"use strict";
const readline = require("readline");
const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
});
const numSides = [2, 4, 6, 8, 10, 12, 20]; //possible side numbers for generated die
let range = [0, 0];
const debt = 500; //how much money  the player needs to make to win
let money = 0; //amount of money that the player has
let remainingTurns = 20; //number of turns that the player has to make up the debt before they lose
let dice = []; //dice that the player has and can roll
let newDieCost = 30; //cost of the next new die that the player can buy
const CalculateRange = () => {
    let highest = 0;
    let lowest = 999999999;
    dice.forEach((d => {
        d.sides.forEach((s) => {
            if (s < lowest) {
                lowest = s;
            }
            if (s > highest) {
                highest = s;
            }
        });
    }));
    range = [lowest, highest];
};
const GenerateDie = (numSides) => {
    let dieOut = {
        sides: []
    };
    for (let i = 1; i <= numSides; i++) {
        dieOut.sides.push(i);
    }
    return dieOut;
};
const AddDie = () => {
    dice.push(GenerateDie(numSides[Math.floor(Math.random() * numSides.length)]));
    CalculateRange();
};
const PrintDiceList = () => {
    console.log(`Your Dice:`);
    dice.forEach((d) => console.log(`${d.sides.length} Sides: ${d.sides}`));
};
const PrintOptions = () => {
    console.log(`[S]pin | [B]uy a random die (Cost: ${newDieCost}) | [V]iew your Dice`);
};
const PrintInfo = () => {
    if (money >= debt) { //case for when the player wins the game by getting enough money
        console.log(`----------------------------------------------`);
        console.log(`Your Debt: ${debt} | Your Money: ${money}`);
        console.log(`You Win`);
        rl.close();
        return;
    }
    if (remainingTurns === 0) { //case for when the player runs out of turns
        console.log(`----------------------------------------------`);
        console.log(`Remaining Turns: 0`);
        console.log(`You Lose`);
        rl.close();
        return;
    }
    console.log(`----------------------------------------------`);
    console.log(`Your Debt: ${debt} | Your Money: ${money} | Your Remaining Turns: ${remainingTurns}`);
    PrintOptions();
};
const Roll = (die) => {
    return die.sides[Math.floor(Math.random() * die.sides.length)];
};
const Spin = () => {
    let results = [];
    let totalPayout = 0;
    dice.forEach((d) => results.push(Roll(d)));
    for (let i = range[0]; i <= range[1]; i++) {
        let numberOfInstances = 0;
        results.forEach((r) => numberOfInstances += +(r === i));
        if (numberOfInstances > 1) {
            totalPayout += i * (numberOfInstances - 1) * 10;
        }
    }
    console.log(results);
    console.log(`Total Payout: ${totalPayout}`);
    money += totalPayout;
    remainingTurns--;
    PlayerAction();
};
const Buy = () => {
    if (money >= newDieCost) {
        money -= newDieCost;
        newDieCost += 10;
        AddDie();
    }
    else {
        console.log(`Insufficient Funds`);
    }
    PrintDiceList();
    PlayerAction();
};
const PlayerAction = () => {
    PrintInfo();
    rl.question(": ", (answer) => {
        switch (answer.toLowerCase().charAt(0)) {
            case 's':
                Spin();
                break;
            case 'b':
                Buy();
                break;
            case 'v':
                PrintDiceList();
                PlayerAction();
                break;
            default:
                console.log(`default case`);
                PlayerAction();
                break;
        }
    });
};
const init = (startingDice) => {
    for (let i = 0; i < startingDice; i++) {
        AddDie();
    }
    PlayerAction();
};
init(3);
//# sourceMappingURL=game.js.map