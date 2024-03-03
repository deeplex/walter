// Copyright (c) 2023-2024 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

import { expect, describe, it } from 'vitest';
import { convertDateCanadian, convertDateGerman } from '$walter/services/utils';

describe('convertDateCanadian tests', () => {
    it('should be 2023-05-24', () => {
        const date = new Date('2023-05-24');

        const convertedDate = convertDateCanadian(date);

        expect(convertedDate).toBe('2023-05-24');
    });
});

describe('convertDateGerman tests', () => {
    it('should be 24.05.2023', () => {
        const date = new Date('2023-05-24');

        const convertedDate = convertDateGerman(date);

        expect(convertedDate).toBe('24.05.2023');
    });
});
