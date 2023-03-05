<script lang="ts">
	import { NumberInput, NumberInputSkeleton } from 'carbon-components-svelte';

	export let value: Promise<number | undefined> | undefined = undefined;
	export let binding: number | undefined = undefined;
	export let label: string | undefined;
	export let hideSteppers: boolean = true;
	export let digits: number = 2;

	function change(e: CustomEvent<number | null>) {
		binding = e.detail || undefined;
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
