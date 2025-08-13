/**
 * EICAR Test Virus File Generator
 * 
 * EICAR (European Institute for Computer Antivirus Research) standard test file
 * This is a harmless test file that all antivirus software should detect as a threat
 * 
 * EICAR string: X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*
 */

export const EICAR_STRING = 'X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*';

/**
 * Creates a File object containing the EICAR test virus
 * @param {string} filename - Name for the test file (default: eicar_test.txt)
 * @returns {File} File object containing EICAR test virus
 */
export function createEicarTestFile(filename = 'eicar_test.txt') {
    const blob = new Blob([EICAR_STRING], { type: 'text/plain' });
    return new File([blob], filename, { type: 'text/plain' });
}

/**
 * Creates multiple EICAR test files with different names
 * @param {number} count - Number of files to create
 * @param {string} prefix - Prefix for filenames (default: eicar_test)
 * @returns {File[]} Array of File objects
 */
export function createMultipleEicarFiles(count = 3, prefix = 'eicar_test') {
    const files = [];
    for (let i = 1; i <= count; i++) {
        const filename = `${prefix}_${i}.txt`;
        files.push(createEicarTestFile(filename));
    }
    return files;
}

/**
 * Creates EICAR test files with different extensions
 * @returns {File[]} Array of File objects with different extensions
 */
export function createEicarFilesWithDifferentExtensions() {
    const extensions = ['txt', 'com', 'exe', 'bat', 'cmd', 'scr', 'pif'];
    return extensions.map(ext => createEicarTestFile(`eicar_test.${ext}`));
}

/**
 * Validates if a file contains the EICAR string
 * @param {File} file - File to validate
 * @returns {Promise<boolean>} True if file contains EICAR string
 */
export async function isEicarFile(file) {
    try {
        const text = await file.text();
        return text.includes(EICAR_STRING);
    } catch (error) {
        console.error('Error validating EICAR file:', error);
        return false;
    }
}

/**
 * Gets information about EICAR test files
 * @returns {Object} Information about EICAR test files
 */
export function getEicarInfo() {
    return {
        description: 'EICAR Standard Antivirus Test File',
        purpose: 'Test antivirus detection without using real malware',
        safety: 'Completely harmless - contains no executable code',
        detection: 'Should be detected by all antivirus software',
        string: EICAR_STRING,
        length: EICAR_STRING.length,
        usage: 'Use for testing antivirus integration and file scanning'
    };
} 