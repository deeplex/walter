<script lang="ts">
    import { NumberInput, NumberInputSkeleton } from 'carbon-components-svelte';

    export let value: number | undefined = undefined;
    export let label: string | undefined;
    export let min: number | undefined = undefined;
    export let max: number | undefined = undefined;
    export let hideSteppers = true;
    export let digits = 2;
    export let readonly = false;
    export let required = false;

    export let change = (e: CustomEvent<number | null>) => {
        if (e.detail === 0) {
            value = 0;
        }
        else
        {
            value = e.detail || undefined;
        }
    };

</script>

{#await value}
    <NumberInputSkeleton />
{:then x}
    <NumberInput
        invalid={required && value !== 0 && value === undefined}
        invalidText={`${label} ist ein notwendiges Feld.`}
        {readonly}
        {min}
        {max}
        {hideSteppers}
        on:change={change}
        {label}
        value={+(x || 0).toFixed(digits)}
    />
{/await}
