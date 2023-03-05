<script lang="ts">
	import {
		WalterDatePicker,
		WalterNumberInput,
		WalterTextInput
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterMietminderungEntry } from '$WalterTypes';

	export let a: Promise<Partial<WalterMietminderungEntry>> | undefined =
		undefined;
	export let entry: Partial<WalterMietminderungEntry> = {};
</script>

<Row>
	<WalterNumberInput
		bind:binding={entry.minderung}
		label="Betrag"
		value={a?.then((x) => x.minderung)}
	/>
	<WalterDatePicker
		bind:binding={entry.beginn}
		value={a?.then((x) => x.beginn)}
		labelText="Beginn"
	/>
	<WalterDatePicker
		placeholder="Offen"
		bind:binding={entry.ende}
		value={a?.then((x) => x.ende)}
		labelText="Ende"
	/>
</Row>
<Row>
	<WalterTextInput
		labelText="Notiz"
		bind:binding={entry.notiz}
		value={a?.then((x) => x.notiz)}
	/>
</Row>
