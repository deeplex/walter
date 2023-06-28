import { convertDateGerman } from '$walter/services/utils';

export function getDateForDatePicker(date: string | undefined) {
    const retval = date ? convertDateGerman(new Date(date)) : undefined;

    return retval;
}
