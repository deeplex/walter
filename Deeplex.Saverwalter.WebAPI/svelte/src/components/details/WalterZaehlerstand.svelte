<script lang="ts">
	import {
		WalterDatePicker,
		WalterNumberInput,
		WalterTextInput
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterZaehlerstandEntry } from '$WalterTypes';

	export let a:
		| Promise<Partial<WalterZaehlerstandEntry | undefined>>
		| undefined = undefined;
	export let entry: Partial<WalterZaehlerstandEntry> = {};
</script>

<Row>
	<WalterNumberInput
		bind:binding={entry.stand}
		label="Zählerstand"
		value={a?.then((x) => x?.stand)}
	/>
	<WalterDatePicker
		bind:binding={entry.datum}
		value={a?.then((x) => x?.datum)}
		labelText="Ablesedatum"
	/>
	<!-- TODO add einheit? -->
</Row>
<Row>
	<WalterTextInput
		labelText="Notiz"
		bind:binding={entry.notiz}
		value={a?.then((x) => x?.notiz)}
	/>
</Row>
