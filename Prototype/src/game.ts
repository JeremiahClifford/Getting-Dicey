declare var require: any
const readline: any = require("readline")

declare var process: any
const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
})

type Die = {
    "sides": number[]
}

const numSides: number[] = [2, 4, 6, 8, 10, 12, 20] //possible side numbers for generated die

let range: number[] = [0, 0]

const debt: number = 500 //how much money  the player needs to make to win
let money: number = 0 //amount of money that the player has
let remainingTurns: number = 20 //number of turns that the player has to make up the debt before they lose
let dice: Die[] = [] //dice that the player has and can roll

let newDieCost: number = 30 //cost of the next new die that the player can buy

const CalculateRange = (): void => { //finds the highest and lowest possible single die result for optimization during the result parsing
    let highest = 0
    let lowest = 999999999

    dice.forEach((d => {
        d.sides.forEach((s) => {
            if (s < lowest) {
                lowest = s
            }
            if (s > highest) {
                highest = s
            }
        })
    }))

    range = [lowest, highest]
}

const GenerateDie = (numSides: number): Die => { //creates a new random die
    let dieOut: Die = {
        sides: []
    }

    for (let i = 1; i <= numSides; i++) {
        dieOut.sides.push(i)
    }
    
    return dieOut
}

const AddDie = (): void => { //adds a die to the player's list of dice and recalculates the range of possible results
    dice.push(GenerateDie(numSides[Math.floor(Math.random() * numSides.length)]))
    CalculateRange()
}

const PrintDiceList = (): void => {
    console.log(`Your Dice:`)
    dice.forEach((d) => console.log(`${d.sides.length} Sides: ${d.sides}`))
    PlayerAction()
}

const PrintOptions = (): void => {
    console.log(`[S]pin | [B]uy a random die (Cost: ${newDieCost}) | [V]iew your Dice`)
}

const PrintInfo = (): void => {
    if (money >= debt) { //case for when the player wins the game by getting enough money
        console.log(`----------------------------------------------`)
        console.log(`Your Debt: ${debt} | Your Money: ${money}`)
        console.log(`You Win`)
        rl.close()
        return
    }

    if (remainingTurns === 0) { //case for when the player runs out of turns
        console.log(`----------------------------------------------`)
        console.log(`Remaining Turns: 0`)
        console.log(`You Lose`)
        rl.close()
        return
    }
    
    console.log(`----------------------------------------------`)
    console.log(`Your Debt: ${debt} | Your Money: ${money} | Your Remaining Turns: ${remainingTurns}`)
    PrintOptions()
}

const Roll = (die: Die): number => {
    return die.sides[Math.floor(Math.random() * die.sides.length)]
}

const Spin = (): void => {
    let results: number[] = []
    let totalPayout: number = 0
    dice.forEach((d) => results.push(Roll(d)))
    for (let i = range[0]; i <= range[1]; i++) {
        let numberOfInstances: number = 0
        results.forEach((r) => numberOfInstances += +(r === i))
        if (numberOfInstances > 1) {
            totalPayout += i * (numberOfInstances - 1) * 10
        }
    }
    console.log(results)

    console.log(`Total Payout: ${totalPayout}`)
    money += totalPayout

    remainingTurns--
    PlayerAction()
}

const Buy = (): void => {
    if (money >= newDieCost) {
        money -= newDieCost
        newDieCost += 10
        AddDie()
    } else {
        console.log(`Insufficient Funds`)
    }

    PlayerAction()
}

const PlayerAction = (): void => {
    PrintInfo()
    rl.question(": ", (answer: string) => {
        switch (answer.toLowerCase().charAt(0)) {
            case 's':
                Spin()
                break
            case 'b':
                Buy()
                break
            case 'v':
                PrintDiceList()
                break
            default:
                console.log(`default case`)
                PlayerAction()
                break
        }
    })
}

const init = (startingDice: number): void => { //starts the game
    for (let i = 0; i < startingDice; i++) {
        AddDie()
    }

    PlayerAction()
}

init(3)