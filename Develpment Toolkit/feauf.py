# This script reads a DN Autrag Import File (*.dnfa) and splits it into two files:
# one containing the lines with a 9 character long string as the first element (feaufs) and
# the other containing the remaining lines (auftrags).
# The script then writes the auftrags list to a new file with the same header as the original file
# and the aheader as the 7th line and the feaufs list to another new file with the same header as the original file
# and the fheader as the 7th line.
# The user is prompted to enter the input filename and output filename if they are not provided as command line arguments.
# The output filename is appended with .dnfa and .dnff extensions for the auftrags and feaufs files respectively.
# The script uses utf-8 encoding and \r\n as the line separator.

import sys

# Define the header strings for the output files
fheader = 'AWUWF0TCRTAEG1FBANUCFNHENKELVISIONNJCEP8XHDNHENKELVISIONW2P1R7QTDNHENKELVISIONR77KK0EBDNHENKELVISION'
aheader = 'AWUWF0TCRTAEG1PB9NACDNHENKELVISIONNJCEP8XHDNHENKELVISIONW2P1R7QTDNHENKELVISIONR77KK0EBDNHENKELVISION'

# Initialize empty lists for the auftrags and feaufs lines
auftrags = []
feaufs = []

# Check if the input filename is provided as a command line argument
filename = ''
if len(sys.argv) > 1:
    filename = sys.argv[1]

# If the input filename is not provided or does not end with .dnfa, prompt the user to enter the filename
if filename[-5:] != '.dnfa':
    filename = input('Enter the filename (*.dnfa): ')

# Read the content of the input file
content = open(filename, 'r', encoding='utf-8').read()

# Split the content into lines
lines = content.split('\n')

# Check if the file has a valid header
if lines[6] != aheader:
    print('This is not a valid DN Autrag Import File file')
    exit()

# Iterate over the lines and append them to the auftrags or feaufs list depending on the first element
for line in lines[7:]:
    if len(line.split('\t')[0]) == 9:
        feaufs.append(line)
    else:
        auftrags.append(line)

# Check if the output filename is provided as a command line argument
outputfilename = ''
if len(sys.argv) > 2:
    outputfilename = sys.argv[2]

# If the output filename is not provided or does not end with .dnfa, prompt the user to enter the filename
if outputfilename[-5:] != '.dnfa':
    outputfilename = input('Enter the output filename: ')

# Write the auftrags and feaufs lists to separate output files
open(outputfilename + '.dnfa', 'w', encoding='utf-8').write('\n'.join(line.rstrip() for line in lines[:6]) + '\n' + aheader + '\n' + '\n'.join(line.rstrip() for line in auftrags))
open(outputfilename + '.dnff', 'w', encoding='utf-8').write('\n'.join(line.rstrip() for line in lines[:6]) + '\n' + fheader + '\n' + '\n'.join(line.rstrip() for line in feaufs))