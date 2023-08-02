<script lang="ts">
    import { NumberInput, NumberInputSkeleton } from 'carbon-components-svelte';

    export let value: number | undefined = undefined;
    export let label: string | undefined;
    export let min: number | undefined = undefined;
    export let max: number | undefined = undefined;
    export let hideSteppers = true;
    export let digits = 2;
    export let readonly = false;

    export let change = (e: CustomEvent<number | null>) => {
        value = e.detail || undefined;
    };
</script>

{#await value}
    <NumberInputSkeleton />
{:then x}
    <NumberInput
        {readonly}
        {min}
        {max}
        {hideSteppers}
        on:change={change}
        {label}
        value={+(x || 0).toFixed(digits)}
    />
{/await}
