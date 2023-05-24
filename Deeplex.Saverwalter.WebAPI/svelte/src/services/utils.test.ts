import { expect, describe, it, afterEach } from 'vitest';
import { convertDateCanadian, convertDateGerman } from '$WalterServices/utils';

describe("convertDateCanadian tests", () => {
    it("should be 2023-05-24", () => {
        const date = new Date("2023-05-24");

        const convertedDate = convertDateCanadian(date);

        expect(convertedDate).toBe("2023-05-24");
    });
});

describe("convertDateGerman tests", () => {
    it("should be 24.05.2023", () => {
        const date = new Date("2023-05-24");

        const convertedDate = convertDateGerman(date);

        expect(convertedDate).toBe("24.05.2023")
    })
});