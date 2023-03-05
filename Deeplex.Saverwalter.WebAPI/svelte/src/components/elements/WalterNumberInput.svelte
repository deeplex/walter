<script lang="ts">
	import { NumberInput, NumberInputSkeleton } from 'carbon-components-svelte';

	export let value: number | undefined = undefined;
	export let label: string | undefined;
	export let hideSteppers: boolean = true;
	export let digits: number = 2;

	function change(e: CustomEvent<number | null>) {
		value = e.detail || undefined;
	}
</script>

{#await value}
	<NumberInputSkeleton />
{:then x}
	<NumberInput
		{hideSteppers}
		on:change={change}
		{label}
		value={+(x || 0).toFixed(digits)}
	/>
{/await}
