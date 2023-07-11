import sys

# Headers
fheader = 'AWUWF0TCRTAEG1FBANUCFNHENKELVISIONNJCEP8XHDNHENKELVISIONW2P1R7QTDNHENKELVISIONR77KK0EBDNHENKELVISION'
aheader = 'AWUWF0TCRTAEG1PB9NACDNHENKELVISIONNJCEP8XHDNHENKELVISIONW2P1R7QTDNHENKELVISIONR77KK0EBDNHENKELVISION'

# Variables
auftrags = []
feaufs = []

filename = ''
outputfilename = ''

# Check if the first argument is valid *.dnfa file and if not, ask for the filename again
if len(sys.argv) > 1:
    filename = sys.argv[1]

if filename[-5:] != '.dnfa':
    filename = input('Enter the filename (*.dnfa): ')

content = open(filename, 'r', encoding='utf-8').read()

# Split the file into lines

lines = content.split('\n')

# Check if the file is a dnfa or dnfn file if not exit the program
# by comparing the 7th line of the file with the header



if lines[6] != aheader:
    print('This is not a valid DN Autrag Import File file')
    exit()

# For each line from the 8th line to the end of the file, check if the first
# element separated by the tab is a 9 character long string and if it is,
# append it to the feaufs list, otherwise append it to the auftrags list

for line in lines[7:]:
    if len(line.split('\t')[0]) == 9:
        feaufs.append(line)
    else:
        auftrags.append(line)

# Ask for the output filename and append the .dnfa extension to it and write the auftrags list to it using utf-8 encoding
# Now, append the .dnff extension to the output filename and write the feaufs list to it using utf-8 encoding
# Use the first 6 lines of the original file as the header for both files and for the auftrags file, use the aheader
# and for the feaufs file, use the fheader and after it, write the auftrags list to the auftrags file and the feaufs list to the feaufs file
# Use \r\n as the line separator

# If the second argument is valid, use it as the output filename
if len(sys.argv) > 2:
    outputfilename = sys.argv[2]

if outputfilename[-5:] != '.dnfa':
    outputfilename = input('Enter the output filename: ')

open(outputfilename + '.dnfa', 'w', encoding='utf-8').write('\n'.join(line.rstrip() for line in lines[:6]) + '\n' + aheader + '\n' + '\n'.join(line.rstrip() for line in auftrags))
open(outputfilename + '.dnff', 'w', encoding='utf-8').write('\n'.join(line.rstrip() for line in lines[:6]) + '\n' + fheader + '\n' + '\n'.join(line.rstrip() for line in feaufs))