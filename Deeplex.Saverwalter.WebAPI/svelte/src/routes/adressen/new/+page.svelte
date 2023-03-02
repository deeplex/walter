<script lang="ts">
	import {
		WalterAdresse,
		WalterGrid,
		WalterHeaderNew,
		WalterWohnungen
	} from '$WalterComponents';
	import type { WalterAdresseEntry } from '$WalterTypes';

	const url = `/api/adressen`;
	const title = 'Neue Adresse';

	const entry: Partial<WalterAdresseEntry> = {};
</script>

<WalterHeaderNew {url} {entry} {title} />

<WalterGrid>
	<WalterAdresse {entry} />
</WalterGrid>
