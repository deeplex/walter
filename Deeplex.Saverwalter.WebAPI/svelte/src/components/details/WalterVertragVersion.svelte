<script lang="ts">
	import {
		WalterDatePicker,
		WalterNumberInput,
		WalterTextInput
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterVertragVersionEntry } from '$WalterTypes';

	export let a:
		| Promise<Partial<WalterVertragVersionEntry | undefined>>
		| undefined = undefined;
	export let entry: Partial<WalterVertragVersionEntry> = {};
</script>

<Row>
	<WalterDatePicker
		bind:binding={entry.beginn}
		value={a?.then((x) => x?.beginn)}
		labelText="Beginn"
	/>
	<WalterNumberInput
		hideSteppers
		bind:binding={entry.grundmiete}
		label="Grundmiete"
		value={a?.then((x) => x?.grundmiete)}
	/>
	<WalterNumberInput
		bind:binding={entry.personenzahl}
		label="Personenzahl"
		value={a?.then((x) => x?.personenzahl)}
	/>
</Row>
<Row>
	<WalterTextInput
		labelText="Notiz"
		bind:binding={entry.notiz}
		value={a?.then((x) => x?.notiz)}
	/>
</Row>
