<script lang="ts">
    import {
        DatePicker,
        DatePickerInput,
        DatePickerSkeleton
    } from 'carbon-components-svelte';

    import {
        convertDateCanadian,
        convertDateGerman
    } from '$walter/services/utils';

    export let labelText: string;
    export let value: string | undefined = undefined;
    export let placeholder: string | undefined = undefined;
    export let disabled: boolean | undefined = false;

    function change(e: CustomEvent) {
        value = convertDateCanadian(new Date(e.detail?.selectedDates[0]));
    }

    function getDateForDatePicker(date: string | undefined) {
        const retval = date ? convertDateGerman(new Date(date)) : undefined;

        return retval;
    }
</script>

{#await value}
    <DatePickerSkeleton />
{:then x}
    <DatePicker
        value={getDateForDatePicker(x)}
        dateFormat="d.m.Y"
        datePickerType="single"
        on:change={change}
    >
        <DatePickerInput
            {disabled}
            style="width: 100%"
            type="text"
            {placeholder}
            {labelText}
        />
    </DatePicker>
{/await}
