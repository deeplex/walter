<script lang="ts">
	import { TextInput, TextInputSkeleton } from 'carbon-components-svelte';

	export let value: Promise<string> | undefined = undefined;
	export let binding: string | undefined = undefined;
	export let labelText: string | undefined;
	export let readonly: boolean = false;

	export let change = (e: CustomEvent<string | number | null>) => {
		binding = '' + e.detail || undefined;
	};
</script>

{#await value}
	<TextInputSkeleton />
{:then x}
	<TextInput {readonly} on:change={change} {labelText} value={x} />
{/await}
