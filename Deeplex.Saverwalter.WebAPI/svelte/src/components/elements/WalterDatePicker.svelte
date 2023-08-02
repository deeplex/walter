<script lang="ts">
    import {
        DatePicker,
        DatePickerInput,
        DatePickerSkeleton
    } from 'carbon-components-svelte';

    import { convertDateCanadian } from '$walter/services/utils';
    import { getDateForDatePicker } from './WalterDatePicker';

    export let labelText: string;
    export let value: string | undefined = undefined;
    export let placeholder: string | undefined = undefined;
    export let disabled: boolean | undefined = false;
    export let required = false;

    function change(e: any) {
        const germanDate = e.target.value;
        if (!germanDate) {
            value = undefined;
        }
        else
        {
            const [day, month, year] = germanDate.split(".");
            // month starts at 0. So 1 has to be substracted
            value = convertDateCanadian(new Date(year, month - 1, day));
        }
    }
</script>

{#await value}
    <DatePickerSkeleton />
{:then x}
    <DatePicker
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
