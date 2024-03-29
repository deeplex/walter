<!-- Copyright (C) 2023-2024  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<script lang="ts">
    import {
        DatePicker,
        DatePickerInput,
        DatePickerSkeleton
    } from 'carbon-components-svelte';

    import {
        convertDateCanadian,
        walter_subscribe_reset_changeTracker,
        walter_update_value
    } from '$walter/services/utils';
    import { getDateForDatePicker } from './WalterDatePicker';
    import { onMount } from 'svelte';

    export let labelText: string;
    export let value: string | undefined = undefined;
    export let placeholder: string | undefined = undefined;
    export let disabled: boolean | undefined = false;
    export let required = false;
    export let maxDate: string | undefined = undefined;

    let lastSavedValue: string | undefined;
    function updateLastSavedValue() {
        lastSavedValue = value;
    }

    onMount(() => {
        walter_subscribe_reset_changeTracker(updateLastSavedValue);
        updateLastSavedValue();
    });

    function change(e: Event) {
        const germanDate = (e.target as HTMLSelectElement).value;
        if (!germanDate) {
            value = walter_update_value(lastSavedValue, value, undefined);
        } else {
            const [day, month, year] = germanDate.split('.');
            // month starts at 0. So 1 has to be substracted
            const new_value = convertDateCanadian(
                new Date(+year, +month - 1, +day)
            );
            value = walter_update_value(lastSavedValue, value, new_value);
        }
    }
</script>

{#await value}
    <DatePickerSkeleton />
{:then x}
    <DatePicker
        {maxDate}
        value={getDateForDatePicker(x)}
        dateFormat="d.m.Y"
        datePickerType="single"
    >
        <DatePickerInput
            on:input={change}
            invalid={required && !value}
            invalidText={`${labelText} ist ein notwendiges Feld.`}
            {disabled}
            style="width: 100%"
            type="text"
            {placeholder}
            {labelText}
        />
    </DatePicker>
{/await}
