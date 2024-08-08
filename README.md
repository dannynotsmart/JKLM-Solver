# JKLM-Solver
A external solving tool for [JKLM.fun](https://jklm.fun/)'s Bomb Party gamemode. **READ THE DISCLAIMER (just right below, for your convenience!)**

## Disclaimer
This project is purely for educational purposes (learning .NET's WPF framework.) I am not responsible nor do I condone any cheating for any purposes. You are responsible for whatever you do with this project. Also, you will most likely lose to an actual web extension cheat bot. Also, this program will not work for every prompt (click [here](#warning-words) for more information.)

## Features
- Tool window is always on top, making it easy to switch between windows.
- Auto-type and auto-enter words.
- Highly customizable options, from typing speed to word lengths.

## Warning: Words 
The true word list used by Bomb Party is private, to prevent cheating. As a result, I am using public dictionary word lists to serve as a source of words. Some of these words will not work on Bomb Party. 

## Installation
TBD

## Technical Details
This program is made in C#, using .NET's WPF framework. The [Keyboard.cs](./Keyboard.cs) file was taken from [this gist](https://gist.github.com/DrustZ/640912b9d5cb745a3a56971c9bd58ac7).

The word lists are stored in `...\AppData\JKLM\words`. The file formats are: `words_{length of words}.json`, and the JSON file consists of a array of words with that length. For example, `words_4.json` contains a array of only 4 letter words. It is easy to modify the word list by changing these files.