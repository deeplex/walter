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
    import monthSelectPlugin from 'flatpickr/dist/plugins/monthSelect';
    import 'flatpickr/dist/plugins/monthSelect/style.css';

    import {
        walter_subscribe_reset_changeTracker,
        walter_update_value
    } from '$walter/services/utils';
    import {
        getDateForMonthPicker,
        parseMonthPickerValue
    } from './WalterMonthPicker';
    import { onMount } from 'svelte';

    export let labelText: string;
    export let value: string | undefined = undefined;
    export let placeholder: string | undefined = 'MM.JJJJ';
    export let disabled: boolean | undefined = false;
    export let required = false;
    export let maxDate: string | undefined = undefined;
    export let minDate: string | undefined = undefined;

    let lastSavedValue: string | undefined;

    function updateLastSavedValue() {
        lastSavedValue = value;
    }

    onMount(() => {
        walter_subscribe_reset_changeTracker(updateLastSavedValue);
        updateLastSavedValue();
    });

    function updateValue(nextDisplayValue: string) {
        const nextValue = parseMonthPickerValue(nextDisplayValue);

        if (!nextDisplayValue) {
            value = walter_update_value(lastSavedValue, value, undefined);
            return;
        }

        if (nextValue) {
            value = walter_update_value(lastSavedValue, value, nextValue);
        }
    }

    function onInput(event: Event) {
        updateValue((event.target as HTMLInputElement).value);
    }
</script>

{#await value}
    <DatePickerSkeleton />
{:then x}
    <DatePicker
        value={getDateForMonthPicker(x)}
        datePickerType="single"
        dateFormat="m.Y"
        {minDate}
        {maxDate}
        flatpickrProps={{
            static: true,
            monthSelectorType: 'static',
            plugins: [
                monthSelectPlugin({
                    dateFormat: 'm.Y',
                    altFormat: 'm.Y',
                    shorthand: true,
                    theme: 'light'
                })
            ]
        }}
    >
        <DatePickerInput
            on:input={onInput}
            invalid={required && !value}
            invalidText={`${labelText} ist ein notwendiges Feld.`}
            {disabled}
            style="width: 100%"
            type="text"
            pattern={'\\d{1,2}\\.\\d{4}'}
            {placeholder}
            {labelText}
        />
    </DatePicker>
{/await}
