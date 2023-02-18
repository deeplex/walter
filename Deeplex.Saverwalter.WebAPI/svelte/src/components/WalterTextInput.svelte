<script lang="ts">
	import { TextInput, TextInputSkeleton } from 'carbon-components-svelte';

	export let value: Promise<string | number> | string | number | undefined;
	export let labelText: string | undefined;
</script>

{#await value}
	<TextInputSkeleton />
{:then x}
	<TextInput {labelText} value={x} />
{/await}
