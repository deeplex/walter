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
